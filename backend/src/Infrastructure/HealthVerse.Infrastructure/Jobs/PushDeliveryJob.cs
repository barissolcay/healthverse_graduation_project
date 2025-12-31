using HealthVerse.Contracts.Notifications;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Push notification delivery job.
/// Processes pending notification deliveries and sends them via FCM.
/// Runs every 30 seconds.
/// 
/// Features:
/// - Batch processing (100 at a time)
/// - Retry with exponential backoff (1m → 5m → 30m)
/// - Invalid token handling (disables device)
/// - DND quiet hours (22:00-08:00 TR) - reschedules to next morning
/// </summary>
[DisallowConcurrentExecution]
public sealed class PushDeliveryJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly INotificationDeliveryRepository _deliveryRepository;
    private readonly IUserDeviceRepository _deviceRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPushSender _pushSender;
    private readonly IClock _clock;
    private readonly ILogger<PushDeliveryJob> _logger;

    private const int BatchSize = 100;
    private static readonly TimeSpan[] RetryBackoffs = { 
        TimeSpan.FromMinutes(1), 
        TimeSpan.FromMinutes(5), 
        TimeSpan.FromMinutes(30) 
    };

    // DND (Do Not Disturb) quiet hours: 22:00-08:00 TR
    private const int QuietHourStart = 22;
    private const int QuietHourEnd = 8;

    public PushDeliveryJob(
        HealthVerseDbContext dbContext,
        INotificationDeliveryRepository deliveryRepository,
        IUserDeviceRepository deviceRepository,
        INotificationRepository notificationRepository,
        IPushSender pushSender,
        IClock clock,
        ILogger<PushDeliveryJob> logger)
    {
        _dbContext = dbContext;
        _deliveryRepository = deliveryRepository;
        _deviceRepository = deviceRepository;
        _notificationRepository = notificationRepository;
        _pushSender = pushSender;
        _clock = clock;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _clock.UtcNow;
        _logger.LogDebug("PushDeliveryJob started at {Time}", now);

        try
        {
            // Get pending deliveries
            var deliveries = await _deliveryRepository.GetReadyToSendAsync(now, BatchSize);
            
            if (deliveries.Count == 0)
            {
                _logger.LogDebug("No pending deliveries to process");
                return;
            }

            _logger.LogInformation("Processing {Count} pending deliveries", deliveries.Count);

            var sentCount = 0;
            var failedCount = 0;
            var deferredCount = 0;

            foreach (var delivery in deliveries)
            {
                try
                {
                    // Check DND quiet hours
                    if (IsQuietHours())
                    {
                        var nextMorning = GetNextMorning();
                        delivery.Reschedule(nextMorning);
                        await _deliveryRepository.UpdateAsync(delivery);
                        deferredCount++;
                        continue;
                    }

                    // Get user's active devices
                    var devices = await _deviceRepository.GetActiveByUserAsync(delivery.UserId);
                    
                    if (devices.Count == 0)
                    {
                        // No devices - mark as failed (no retry)
                        delivery.RecordFailedAttempt("NoDeviceToken");
                        await _deliveryRepository.UpdateAsync(delivery);
                        failedCount++;
                        continue;
                    }

                    // Get notification content
                    var notification = await _notificationRepository.GetByIdAsync(delivery.NotificationId);
                    if (notification == null)
                    {
                        delivery.Cancel();
                        await _deliveryRepository.UpdateAsync(delivery);
                        failedCount++;
                        continue;
                    }

                    // Send to all active devices
                    var anySuccess = false;
                    foreach (var device in devices)
                    {
                        var result = await _pushSender.SendAsync(
                            device.PushToken,
                            notification.Title,
                            notification.Body,
                            new Dictionary<string, string>
                            {
                                ["type"] = notification.Type,
                                ["notificationId"] = notification.Id.ToString()
                            });

                        if (result.Success)
                        {
                            anySuccess = true;
                        }
                        else if (result.IsInvalidToken)
                        {
                            // Disable invalid token
                            await _deviceRepository.DisableAsync(device.Id);
                            _logger.LogWarning("Disabled invalid device token: {DeviceId}", device.Id);
                        }
                    }

                    if (anySuccess)
                    {
                        delivery.MarkAsSent(now);
                        sentCount++;
                    }
                    else
                    {
                        // All devices failed - retry with backoff
                        var retryAt = CalculateRetryTime(delivery.AttemptCount, now);
                        delivery.RecordFailedAttempt("AllDevicesFailed", retryAt);
                        failedCount++;
                    }

                    await _deliveryRepository.UpdateAsync(delivery);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing delivery {DeliveryId}", delivery.Id);
                    
                    var retryAt = CalculateRetryTime(delivery.AttemptCount, now);
                    delivery.RecordFailedAttempt(ex.Message, retryAt);
                    await _deliveryRepository.UpdateAsync(delivery);
                    failedCount++;
                }
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "PushDeliveryJob completed. Sent: {Sent}, Failed: {Failed}, Deferred: {Deferred}",
                sentCount, failedCount, deferredCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PushDeliveryJob failed");
            throw;
        }
    }

    private bool IsQuietHours()
    {
        var trTime = _clock.TodayTR.ToDateTime(TimeOnly.FromDateTime(_clock.UtcNow.DateTime.AddHours(3)));
        var hour = trTime.Hour;
        
        // 22:00-23:59 or 00:00-07:59
        return hour >= QuietHourStart || hour < QuietHourEnd;
    }

    private DateTimeOffset GetNextMorning()
    {
        var trDate = _clock.TodayTR;
        var hour = _clock.UtcNow.DateTime.AddHours(3).Hour;
        
        // If after 22:00, next morning is tomorrow
        // If before 08:00, next morning is today at 08:00
        if (hour >= QuietHourStart)
        {
            trDate = trDate.AddDays(1);
        }

        // 08:00 TR = 05:00 UTC
        return new DateTimeOffset(trDate.Year, trDate.Month, trDate.Day, 5, 0, 0, TimeSpan.Zero);
    }

    private DateTimeOffset? CalculateRetryTime(int currentAttemptCount, DateTimeOffset now)
    {
        if (currentAttemptCount >= RetryBackoffs.Length)
        {
            return null; // Max retries reached, will be marked as failed
        }

        var backoff = RetryBackoffs[currentAttemptCount];
        return now.Add(backoff);
    }
}

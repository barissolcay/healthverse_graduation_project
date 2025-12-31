using HealthVerse.Contracts.Notifications;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Notifications.Infrastructure.Services;

/// <summary>
/// Implementation of INotificationService that creates both Notification and NotificationDelivery records.
/// Uses INotificationPushPolicy to decide whether push notifications should be sent.
/// Implements both the cross-module contract (INotificationService) and internal interface (INotificationServiceInternal).
/// </summary>
public sealed class NotificationService : INotificationServiceInternal
{
    private readonly INotificationRepository _notificationRepo;
    private readonly INotificationDeliveryRepository _deliveryRepo;
    private readonly IUserDeviceRepository _deviceRepo;
    private readonly INotificationPushPolicy _pushPolicy;
    private readonly INotificationsUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepo,
        INotificationDeliveryRepository deliveryRepo,
        IUserDeviceRepository deviceRepo,
        INotificationPushPolicy pushPolicy,
        INotificationsUnitOfWork unitOfWork,
        IClock clock,
        ILogger<NotificationService> logger)
    {
        _notificationRepo = notificationRepo;
        _deliveryRepo = deliveryRepo;
        _deviceRepo = deviceRepo;
        _pushPolicy = pushPolicy;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(
        Guid userId,
        string type,
        string title,
        string body,
        Guid? referenceId = null,
        string? referenceType = null,
        string? data = null,
        CancellationToken ct = default)
    {
        var notification = await CreateAndReturnAsync(userId, type, title, body, referenceId, referenceType, data, ct);
        return notification.Id;
    }

    /// <inheritdoc />
    public async Task<Notification> CreateAndReturnAsync(
        Guid userId,
        string type,
        string title,
        string body,
        Guid? referenceId = null,
        string? referenceType = null,
        string? data = null,
        CancellationToken ct = default)
    {
        var now = _clock.UtcNow;

        // 1. Create the in-app notification (always created)
        var notification = Notification.Create(
            userId,
            type,
            title,
            body,
            referenceId,
            referenceType,
            data,
            now
        );

        await _notificationRepo.AddAsync(notification, ct);

        // 2. Check push policy
        var pushDecision = await _pushPolicy.ShouldSendPushAsync(userId, type, now, ct);

        if (pushDecision.ShouldSend || pushDecision.ScheduledAt.HasValue)
        {
            // 3. Get user's active devices
            var devices = await _deviceRepo.GetActiveByUserAsync(userId, ct);

            // 4. Create delivery records for each device
            foreach (var device in devices)
            {
                var scheduledAt = pushDecision.ScheduledAt ?? now;
                var delivery = NotificationDelivery.Create(
                    notification.Id,
                    userId,
                    scheduledAt,
                    DeliveryChannel.Push
                );
                await _deliveryRepo.AddAsync(delivery, ct);
            }

            _logger.LogDebug(
                "Created notification {NotificationId} for user {UserId} with {DeviceCount} delivery targets. Push: {PushDecision}, ScheduledAt: {ScheduledAt}",
                notification.Id,
                userId,
                devices.Count,
                pushDecision.Reason,
                pushDecision.ScheduledAt);
        }
        else
        {
            _logger.LogDebug(
                "Created notification {NotificationId} for user {UserId} without push delivery. Reason: {Reason}",
                notification.Id,
                userId,
                pushDecision.Reason);
        }

        return notification;
    }

    /// <inheritdoc />
    public async Task<List<Guid>> CreateBatchAsync(
        IEnumerable<NotificationCreateRequest> requests,
        CancellationToken ct = default)
    {
        var notifications = await CreateBatchAndReturnAsync(requests, ct);
        return notifications.Select(n => n.Id).ToList();
    }

    /// <inheritdoc />
    public async Task<List<Notification>> CreateBatchAndReturnAsync(
        IEnumerable<NotificationCreateRequest> requests,
        CancellationToken ct = default)
    {
        var now = _clock.UtcNow;
        var notifications = new List<Notification>();
        var requestList = requests.ToList();

        if (requestList.Count == 0)
            return notifications;

        // Get all unique user IDs
        var userIds = requestList.Select(r => r.UserId).Distinct().ToList();

        // Batch check push policy for all users (grouped by notification type)
        var requestsByType = requestList.GroupBy(r => r.Type);
        var pushDecisions = new Dictionary<(Guid UserId, string Type), PushDecision>();

        foreach (var typeGroup in requestsByType)
        {
            var typeUserIds = typeGroup.Select(r => r.UserId).Distinct();
            var decisions = await _pushPolicy.ShouldSendPushBatchAsync(typeUserIds, typeGroup.Key, now, ct);
            
            foreach (var (userId, decision) in decisions)
            {
                pushDecisions[(userId, typeGroup.Key)] = decision;
            }
        }

        // Group by user to optimize device lookups
        var requestsByUser = requestList.GroupBy(r => r.UserId);

        foreach (var userGroup in requestsByUser)
        {
            var userId = userGroup.Key;
            List<UserDevice>? devices = null; // Lazy load

            foreach (var request in userGroup)
            {
                // 1. Create the in-app notification
                var notification = Notification.Create(
                    request.UserId,
                    request.Type,
                    request.Title,
                    request.Body,
                    request.ReferenceId,
                    request.ReferenceType,
                    request.Data,
                    now
                );

                await _notificationRepo.AddAsync(notification, ct);
                notifications.Add(notification);

                // 2. Check push decision for this user+type
                if (pushDecisions.TryGetValue((userId, request.Type), out var decision))
                {
                    if (decision.ShouldSend || decision.ScheduledAt.HasValue)
                    {
                        // Lazy load devices
                        devices ??= await _deviceRepo.GetActiveByUserAsync(userId, ct);

                        // 3. Create delivery records
                        foreach (var device in devices)
                        {
                            var scheduledAt = decision.ScheduledAt ?? now;
                            var delivery = NotificationDelivery.Create(
                                notification.Id,
                                userId,
                                scheduledAt,
                                DeliveryChannel.Push
                            );
                            await _deliveryRepo.AddAsync(delivery, ct);
                        }
                    }
                }
            }
        }

        _logger.LogDebug("Created {Count} notifications in batch", notifications.Count);

        return notifications;
    }
}

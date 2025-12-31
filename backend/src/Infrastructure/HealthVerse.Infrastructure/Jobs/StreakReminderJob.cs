using HealthVerse.Contracts.Notifications;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Günlük streak hatırlatma job'ı.
/// Her gün 17:00 TR saatinde (14:00 UTC) çalışır.
/// 
/// - Bugün 3000 adıma ulaşmamış kullanıcıları tespit et
/// - STREAK_REMINDER bildirimi gönder
/// </summary>
[DisallowConcurrentExecution]
public sealed class StreakReminderJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly ILogger<StreakReminderJob> _logger;

    private const int DAILY_STEP_TARGET = 3000;

    public StreakReminderJob(
        HealthVerseDbContext dbContext,
        INotificationService notificationService,
        IClock clock,
        ILogger<StreakReminderJob> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _clock = clock;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _clock.UtcNow;
        var today = _clock.TodayTR;
        _logger.LogInformation("StreakReminderJob started at {Time} for date {Date}", now, today);

        try
        {
            // Aktif kullanıcıları çek (streak'i 0'dan büyük olanlar)
            var usersWithStreak = await _dbContext.Users
                .Where(u => u.StreakCount > 0)
                .Select(u => new { u.Id, u.StreakCount, u.FreezeInventory })
                .ToListAsync();

            if (!usersWithStreak.Any())
            {
                _logger.LogInformation("No users with active streak found");
                return;
            }

            // Bugünkü adım verilerini çek
            var todayStats = await _dbContext.UserDailyStats
                .Where(s => s.LogDate == today)
                .ToDictionaryAsync(s => s.UserId, s => s.DailySteps);

            var reminderCount = 0;

            foreach (var user in usersWithStreak)
            {
                var todaySteps = todayStats.GetValueOrDefault(user.Id, 0);

                // Zaten hedefi aştıysa atla
                if (todaySteps >= DAILY_STEP_TARGET)
                    continue;

                var remaining = DAILY_STEP_TARGET - todaySteps;

                // Bildirimi oluştur (with delivery)
                await _notificationService.CreateAsync(
                    user.Id,
                    NotificationType.STREAK_REMINDER,
                    "Serin riskte! ⚠️",
                    $"Serini korumak için {remaining:N0} adım kaldı! {(user.FreezeInventory > 0 ? $"({user.FreezeInventory} dondurma hakkın var)" : "Dondurma hakkın yok!")}",
                    null,
                    null,
                    default);
                reminderCount++;
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("StreakReminderJob completed. Sent {Count} reminders", reminderCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StreakReminderJob failed");
            throw;
        }
    }
}

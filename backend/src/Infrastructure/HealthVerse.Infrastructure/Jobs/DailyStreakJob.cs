using HealthVerse.Contracts.Notifications;
using HealthVerse.Gamification.Domain.Services;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Günlük streak değerlendirmesi yapan job.
/// Her gün 00:05 TR saatinde çalışır.
/// 
/// - Dünkü adımları kontrol et
/// - Streak güncelle veya freeze kullan veya sıfırla
/// - Bildirim oluştur (STREAK_FROZEN / STREAK_LOST)
/// </summary>
[DisallowConcurrentExecution]
public sealed class DailyStreakJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly StreakService _streakService;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly ILogger<DailyStreakJob> _logger;

    public DailyStreakJob(
        HealthVerseDbContext dbContext,
        StreakService streakService,
        INotificationService notificationService,
        IClock clock,
        ILogger<DailyStreakJob> logger)
    {
        _dbContext = dbContext;
        _streakService = streakService;
        _notificationService = notificationService;
        _clock = clock;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _clock.UtcNow;
        var yesterday = _clock.TodayTR.AddDays(-1);
        _logger.LogInformation("DailyStreakJob started at {Time} for date {Date}", now, yesterday);

        try
        {
            // Aktif kullanıcıları çek (son 30 günde adım atmış olanlar)
            var thirtyDaysAgo = DateOnly.FromDateTime(_clock.UtcNow.AddDays(-30).DateTime);
            var activeUserIds = await _dbContext.UserDailyStats
                .Where(s => s.LogDate >= thirtyDaysAgo)
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync();

            _logger.LogInformation("Processing {Count} active users", activeUserIds.Count);

            var processedCount = 0;
            var frozenCount = 0;
            var lostCount = 0;

            foreach (var userId in activeUserIds)
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null) continue;

                // Dünkü adımları kontrol et
                var yesterdayStats = await _dbContext.UserDailyStats
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.LogDate == yesterday);

                var yesterdaySteps = yesterdayStats?.DailySteps ?? 0;

                // Streak değerlendir
                var result = _streakService.EvaluateStreak(
                    yesterdaySteps,
                    user.StreakCount,
                    user.FreezeInventory
                );

                // Kullanıcıyı güncelle
                if (result.Action == StreakAction.Increment)
                {
                    user.UpdateStreak(result.NewStreakCount, yesterday);
                }
                else if (result.Action == StreakAction.Frozen)
                {
                    frozenCount++;
                    user.UseFreeze();
                    
                    // Bildirim oluştur (INotificationService ile)
                    await _notificationService.CreateAsync(
                        userId,
                        NotificationType.STREAK_FROZEN,
                        "Streak Donduruldu! ❄️",
                        $"Dün hedefe ulaşamadın ama bir dondurma hakkın kullanıldı. Streak'in: {user.StreakCount} gün"
                    );
                }
                else if (result.Action == StreakAction.Lost && result.StreakLost)
                {
                    lostCount++;
                    user.ResetStreak();
                    
                    // Bildirim oluştur (INotificationService ile)
                    await _notificationService.CreateAsync(
                        userId,
                        NotificationType.STREAK_LOST,
                        "Streak Kaybedildi! 💔",
                        "Dün hedefe ulaşamadın ve dondurma hakkın kalmamıştı. Yeni bir başlangıç zamanı!"
                    );
                }

                processedCount++;
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation(
                "DailyStreakJob completed. Processed: {Processed}, Frozen: {Frozen}, Lost: {Lost}",
                processedCount, frozenCount, lostCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DailyStreakJob failed");
            throw;
        }
    }
}


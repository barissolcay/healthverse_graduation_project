using HealthVerse.Contracts.Notifications;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Haftalık özet job'ı.
/// Pazartesi 09:00 TR'de çalışır.
/// 
/// - WEEKLY_SUMMARY bildirimi: Geçen haftanın özeti
/// - LEAGUE_NEW_WEEK bildirimi: Yeni lig haftası başladı
/// </summary>
[DisallowConcurrentExecution]
public sealed class WeeklySummaryJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly ILogger<WeeklySummaryJob> _logger;

    public WeeklySummaryJob(
        HealthVerseDbContext dbContext,
        INotificationService notificationService,
        IClock clock,
        ILogger<WeeklySummaryJob> logger)
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
        _logger.LogInformation("WeeklySummaryJob started at {Time}", now);

        try
        {
            // Geçen hafta: Pazartesi - Pazar
            var lastWeekStart = today.AddDays(-7);
            var lastWeekEnd = today.AddDays(-1);

            // Tüm aktif kullanıcıları çek
            var users = await _dbContext.Users
                .Select(u => new { u.Id, u.StreakCount, u.CurrentTier })
                .ToListAsync();

            if (!users.Any())
            {
                _logger.LogInformation("No users found");
                return;
            }

            var userIds = users.Select(u => u.Id).ToList();

            // Geçen hafta tamamlanan görevleri say
            var completedTaskCounts = await _dbContext.UserTasks
                .Where(t => userIds.Contains(t.UserId) &&
                            t.Status == UserTaskStatus.REWARD_CLAIMED &&
                            t.CompletedAt >= lastWeekStart.ToDateTime(TimeOnly.MinValue) &&
                            t.CompletedAt < lastWeekEnd.ToDateTime(TimeOnly.MaxValue))
                .GroupBy(t => t.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            // Geçen hafta kazanılan puanları say
            var lastWeekStartDto = new DateTimeOffset(lastWeekStart.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(3));
            var lastWeekEndDto = new DateTimeOffset(lastWeekEnd.ToDateTime(TimeOnly.MaxValue), TimeSpan.FromHours(3));
            
            var earnedPoints = await _dbContext.PointTransactions
                .Where(t => userIds.Contains(t.UserId) &&
                            t.CreatedAt >= lastWeekStartDto &&
                            t.CreatedAt <= lastWeekEndDto)
                .GroupBy(t => t.UserId)
                .Select(g => new { UserId = g.Key, Points = g.Sum(t => t.Amount) })
                .ToDictionaryAsync(x => x.UserId, x => x.Points);

            int sentCount = 0;

            foreach (var user in users)
            {
                var tasksCompleted = completedTaskCounts.GetValueOrDefault(user.Id, 0);
                var pointsEarned = earnedPoints.GetValueOrDefault(user.Id, 0);

                // En az aktivitesi olan kullanıcılara özet gönder
                if (tasksCompleted > 0 || pointsEarned > 0 || user.StreakCount > 0)
                {
                    // Haftalık özet + Yeni lig haftası bildirimi (batch)
                    await _notificationService.CreateBatchAsync(new[]
                    {
                        new NotificationCreateRequest(
                            user.Id,
                            NotificationType.WEEKLY_SUMMARY,
                            "Haftalık özet 📊",
                            $"Geçen hafta: {tasksCompleted} görev, {pointsEarned:N0} puan, {user.StreakCount} gün seri!",
                            null,
                            null),
                        new NotificationCreateRequest(
                            user.Id,
                            NotificationType.LEAGUE_NEW_WEEK,
                            "Yeni lig haftası! 🏟️",
                            $"Bu hafta {user.CurrentTier} liginde mücadele ediyorsun. Hedefin ne?",
                            null,
                            "LEAGUE")
                    }, default);

                    sentCount++;
                }
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("WeeklySummaryJob completed. Sent {Count} summary notifications", sentCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WeeklySummaryJob failed");
            throw;
        }
    }
}

using HealthVerse.Contracts.Notifications;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Social.Domain.Entities;
using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Milestone kontrol job'ı.
/// Günlük çalışır (02:00 TR).
/// 
/// - Kullanıcıların metriklerini kontrol eder
/// - Yeni milestone'lara ulaşanları tespit eder
/// - MILESTONE_BADGE, MILESTONE_TITLE, MILESTONE_FREEZE, MILESTONE_APPROACHING bildirimleri gönderir
/// </summary>
[DisallowConcurrentExecution]
public sealed class MilestoneCheckJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly ILogger<MilestoneCheckJob> _logger;

    public MilestoneCheckJob(
        HealthVerseDbContext dbContext,
        INotificationService notificationService,
        IClock clock,
        ILogger<MilestoneCheckJob> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _clock = clock;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _clock.UtcNow;
        _logger.LogInformation("MilestoneCheckJob started at {Time}", now);

        try
        {
            // Aktif milestone'ları çek
            var milestones = await _dbContext.MilestoneRewards
                .Where(m => m.IsActive)
                .ToListAsync();

            if (!milestones.Any())
            {
                _logger.LogInformation("No active milestones found");
                return;
            }

            // Tüm kullanıcıları çek (streak ve puan bilgileriyle)
            var users = await _dbContext.Users
                .Select(u => new { 
                    u.Id, 
                    u.StreakCount, 
                    u.LongestStreakCount, 
                    u.TotalPoints,
                    u.TotalTasksCompleted,
                    u.TotalDuels,
                    u.TotalDuelsWon,
                    u.TotalGlobalMissions
                })
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            // Mevcut kazanılan milestone'ları çek
            var existingAchievements = await _dbContext.UserMilestones
                .Where(um => userIds.Contains(um.UserId))
                .Select(um => new { um.UserId, um.MilestoneRewardId })
                .ToListAsync();

            var achievedSet = existingAchievements
                .Select(a => (a.UserId, a.MilestoneRewardId))
                .ToHashSet();

            // Ek metrikleri hesapla (UserTask ve UserGoal'den)
            var taskCounts = await _dbContext.UserTasks
                .Where(t => t.Status == UserTaskStatus.REWARD_CLAIMED)
                .GroupBy(t => t.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var goalCounts = await _dbContext.UserGoals
                .Where(g => g.CompletedAt != null)
                .GroupBy(g => g.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            int achievedCount = 0;
            int approachingCount = 0;

            foreach (var user in users)
            {
                foreach (var milestone in milestones)
                {
                    // Zaten kazanılmış mı?
                    if (achievedSet.Contains((user.Id, milestone.Id)))
                        continue;

                    // Kullanıcının bu metrik için değerini al
                    var currentValue = GetMetricValue(
                        milestone.Metric,
                        user.StreakCount,
                        user.LongestStreakCount,
                        user.TotalPoints,
                        user.TotalTasksCompleted,
                        user.TotalDuels,
                        user.TotalDuelsWon,
                        user.TotalGlobalMissions,
                        taskCounts.GetValueOrDefault(user.Id, 0),
                        goalCounts.GetValueOrDefault(user.Id, 0));

                    // Hedefe ulaştı mı?
                    if (currentValue >= milestone.TargetValue)
                    {
                        // Milestone kazanıldı!
                        var userMilestone = UserMilestone.Create(user.Id, milestone.Id);
                        _dbContext.UserMilestones.Add(userMilestone);

                        // Bildirim tipi belirle
                        var notificationType = milestone.RewardType switch
                        {
                            MilestoneRewardType.BADGE => NotificationType.MILESTONE_BADGE,
                            MilestoneRewardType.TITLE => NotificationType.MILESTONE_TITLE,
                            MilestoneRewardType.FREEZE => NotificationType.MILESTONE_FREEZE,
                            _ => NotificationType.MILESTONE_BADGE
                        };

                        var emoji = milestone.RewardType switch
                        {
                            MilestoneRewardType.BADGE => "🏅",
                            MilestoneRewardType.TITLE => "👑",
                            MilestoneRewardType.FREEZE => "❄️",
                            _ => "🎉"
                        };

                        await _notificationService.CreateAsync(
                            user.Id,
                            notificationType,
                            $"Yeni başarı! {emoji}",
                            $"\"{milestone.Title}\" - {milestone.Description}",
                            milestone.Id,
                            "MILESTONE",
                            $"{{\"reward_type\": \"{milestone.RewardType}\", \"points\": {milestone.PointsReward}, \"freeze\": {milestone.FreezeReward}}}"
                        );

                        // Freeze ödülü varsa ekle
                        if (milestone.FreezeReward > 0)
                        {
                            var dbUser = await _dbContext.Users.FindAsync(user.Id);
                            dbUser?.AddFreezeRight(milestone.FreezeReward);
                        }

                        // Puan ödülü varsa ekle
                        if (milestone.PointsReward > 0)
                        {
                            var dbUser = await _dbContext.Users.FindAsync(user.Id);
                            dbUser?.AddPoints(milestone.PointsReward);
                        }

                        achievedSet.Add((user.Id, milestone.Id));
                        achievedCount++;
                    }
                    // Hedefe yaklaştı mı? (%90+)
                    else if (currentValue >= milestone.TargetValue * 0.9 && currentValue > 0)
                    {
                        var remaining = milestone.TargetValue - currentValue;

                        // Son 24 saatte bu milestone için approaching bildirimi gönderilmiş mi?
                        var recentApproaching = await _dbContext.Notifications
                            .AnyAsync(n => n.UserId == user.Id &&
                                          n.Type == NotificationType.MILESTONE_APPROACHING &&
                                          n.ReferenceId == milestone.Id &&
                                          n.CreatedAt >= now.AddDays(-1));

                        if (!recentApproaching)
                        {
                            await _notificationService.CreateAsync(
                                user.Id,
                                NotificationType.MILESTONE_APPROACHING,
                                "Hedefe çok az kaldı! 👀",
                                $"\"{milestone.Title}\" için {remaining} kaldı!",
                                milestone.Id,
                                "MILESTONE"
                            );
                            approachingCount++;
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("MilestoneCheckJob completed. Achieved: {Achieved}, Approaching: {Approaching}",
                achievedCount, approachingCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MilestoneCheckJob failed");
            throw;
        }
    }

    private int GetMetricValue(
        string metric,
        int streakCount,
        int longestStreakCount,
        long totalPoints,
        int totalTasksCompleted,
        int totalDuels,
        int totalDuelsWon,
        int totalGlobalMissions,
        int taskCountFromDb,
        int goalCountFromDb)
    {
        return metric switch
        {
            MilestoneMetric.STREAK_COUNT => streakCount,
            MilestoneMetric.STREAK_MAX => longestStreakCount,
            MilestoneMetric.POINTS_TOTAL => (int)Math.Min(totalPoints, int.MaxValue),
            MilestoneMetric.TASK_COMPLETED => Math.Max(totalTasksCompleted, taskCountFromDb),
            MilestoneMetric.GOAL_COMPLETED => goalCountFromDb,
            MilestoneMetric.DUEL_WIN => totalDuelsWon,
            MilestoneMetric.DUEL_TOTAL => totalDuels,
            MilestoneMetric.GLOBAL_MISSION_COMPLETED => totalGlobalMissions,
            _ => 0
        };
    }
}

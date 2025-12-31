using HealthVerse.Contracts.Notifications;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Tasks.Domain.Entities;
using HealthVerse.Social.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Süresi dolmuş entity'leri expire/fail eden job.
/// Her saat çalışır.
/// 
/// - UserTasks: ValidUntil geçmiş ve ACTIVE → FAILED
/// - Duels: WAITING 24 saat geçmiş → EXPIRED
/// - Duels: ACTIVE ve EndDate geçmiş → FINISHED
/// </summary>
[DisallowConcurrentExecution]
public sealed class ExpireJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly ILogger<ExpireJob> _logger;

    public ExpireJob(
        HealthVerseDbContext dbContext,
        INotificationService notificationService,
        IClock clock,
        ILogger<ExpireJob> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _clock = clock;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _clock.UtcNow;
        _logger.LogInformation("ExpireJob started at {Time}", now);

        try
        {
            // 1. UserTasks: ACTIVE ve ValidUntil geçmiş → FAILED
            await ExpireUserTasks(now);

            // 2. Duels: WAITING ve 24 saat geçmiş → EXPIRED
            await ExpireDuelInvitations(now);

            // 3. Duels: ACTIVE ve EndDate geçmiş → FINISHED
            await FinishExpiredDuels(now);

            _logger.LogInformation("ExpireJob completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ExpireJob failed");
            throw;
        }
    }

    private async Task ExpireUserTasks(DateTimeOffset now)
    {
        var expiredTasks = await _dbContext.UserTasks
            .Where(t => t.Status == UserTaskStatus.ACTIVE && t.ValidUntil <= now)
            .ToListAsync();

        foreach (var task in expiredTasks)
        {
            task.MarkAsFailed();
        }

        if (expiredTasks.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} tasks", expiredTasks.Count);
        }
    }

    private async Task ExpireDuelInvitations(DateTimeOffset now)
    {
        var expiredDuels = await _dbContext.Duels
            .Where(d => d.Status == DuelStatus.WAITING && d.CreatedAt <= now.AddHours(-24))
            .ToListAsync();

        // Kullanıcı bilgilerini çek
        var userIds = expiredDuels.SelectMany(d => new[] { d.ChallengerId, d.OpponentId }).Distinct().ToList();
        var users = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username.Value);

        foreach (var duel in expiredDuels)
        {
            duel.Expire(now);
            
            // Challenger'a bildirim gönder (INotificationService ile)
            var opponentName = users.GetValueOrDefault(duel.OpponentId, "Rakip");
            await _notificationService.CreateAsync(
                duel.ChallengerId,
                NotificationType.DUEL_EXPIRED,
                "Düello isteği düştü",
                $"{opponentName} 24 saat içinde yanıt vermedi. İstek iptal edildi.",
                duel.Id,
                "DUEL"
            );
        }

        if (expiredDuels.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} duel invitations", expiredDuels.Count);
        }
    }

    private async Task FinishExpiredDuels(DateTimeOffset now)
    {
        var finishedDuels = await _dbContext.Duels
            .Where(d => d.Status == DuelStatus.ACTIVE && d.EndDate != null && d.EndDate <= now)
            .ToListAsync();

        // Kullanıcı bilgilerini çek
        var userIds = finishedDuels.SelectMany(d => new[] { d.ChallengerId, d.OpponentId }).Distinct().ToList();
        var users = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username.Value);

        foreach (var duel in finishedDuels)
        {
            duel.Finish(now);
            
            // Her iki tarafa bildirim gönder
            var challengerName = users.GetValueOrDefault(duel.ChallengerId, "Rakip");
            var opponentName = users.GetValueOrDefault(duel.OpponentId, "Rakip");
            
            // Sonuca göre mesaj belirle
            var (winnerTitle, loserTitle, drawTitle) = ("Düello kazandın! 🏆", "Düello bitti", "Düello berabere! 🤝");
            
            if (duel.Result == DuelResult.CHALLENGER_WIN)
            {
                await _notificationService.CreateAsync(duel.ChallengerId, NotificationType.DUEL_FINISHED, winnerTitle, $"{opponentName} ile düelloyu kazandın!", duel.Id, "DUEL");
                await _notificationService.CreateAsync(duel.OpponentId, NotificationType.DUEL_FINISHED, loserTitle, $"{challengerName} ile düello sona erdi.", duel.Id, "DUEL");
            }
            else if (duel.Result == DuelResult.OPPONENT_WIN)
            {
                await _notificationService.CreateAsync(duel.OpponentId, NotificationType.DUEL_FINISHED, winnerTitle, $"{challengerName} ile düelloyu kazandın!", duel.Id, "DUEL");
                await _notificationService.CreateAsync(duel.ChallengerId, NotificationType.DUEL_FINISHED, loserTitle, $"{opponentName} ile düello sona erdi.", duel.Id, "DUEL");
            }
            else
            {
                // BOTH_WIN veya BOTH_LOSE
                await _notificationService.CreateAsync(duel.ChallengerId, NotificationType.DUEL_FINISHED, drawTitle, $"{opponentName} ile düello berabere bitti!", duel.Id, "DUEL");
                await _notificationService.CreateAsync(duel.OpponentId, NotificationType.DUEL_FINISHED, drawTitle, $"{challengerName} ile düello berabere bitti!", duel.Id, "DUEL");
            }
        }

        if (finishedDuels.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Finished {Count} expired duels", finishedDuels.Count);
        }
    }
}

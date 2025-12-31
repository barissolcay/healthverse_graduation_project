using HealthVerse.Contracts.Notifications;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Global Mission bitiş job'ı.
/// Her saat çalışır. Süresi dolan ACTIVE görevleri FINISHED yapar ve bildirim gönderir.
/// 
/// - GLOBAL_MISSION_COMPLETED: Katkı yapan herkese
/// - GLOBAL_MISSION_TOP3: Top 3 kullanıcılara
/// </summary>
[DisallowConcurrentExecution]
public sealed class GlobalMissionFinalizeJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly ILogger<GlobalMissionFinalizeJob> _logger;

    // Katılım ödülü puanı
    private const int PARTICIPATION_POINTS = 50;
    private const int TOP1_BONUS = 100;
    private const int TOP2_BONUS = 75;
    private const int TOP3_BONUS = 50;

    public GlobalMissionFinalizeJob(
        HealthVerseDbContext dbContext,
        INotificationService notificationService,
        IClock clock,
        ILogger<GlobalMissionFinalizeJob> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _clock = clock;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _clock.UtcNow;
        _logger.LogInformation("GlobalMissionFinalizeJob started at {Time}", now);

        try
        {
            // Süresi dolmuş ACTIVE görevleri bul
            var endedMissions = await _dbContext.GlobalMissions
                .Where(m => m.Status == MissionStatus.ACTIVE && m.EndDate <= now)
                .ToListAsync();

            if (!endedMissions.Any())
            {
                _logger.LogInformation("No missions to finalize");
                return;
            }

            foreach (var mission in endedMissions)
            {
                await FinalizeMission(mission);
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("GlobalMissionFinalizeJob completed. Finalized {Count} missions", endedMissions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GlobalMissionFinalizeJob failed");
            throw;
        }
    }

    private async Task FinalizeMission(GlobalMission mission)
    {
        _logger.LogInformation("Finalizing mission: {MissionId} - {Title}", mission.Id, mission.Title);

        // Görevi FINISHED olarak işaretle
        mission.Finish();

        // Katkı yapan katılımcıları çek (sıralı)
        var participants = await _dbContext.GlobalMissionParticipants
            .Where(p => p.MissionId == mission.Id && p.ContributionValue > 0)
            .OrderByDescending(p => p.ContributionValue)
            .ToListAsync();

        if (!participants.Any())
        {
            _logger.LogInformation("No contributors for mission {MissionId}", mission.Id);
            return;
        }

        // Top 3 kullanıcıları belirle
        var top3 = participants.Take(3).ToList();

        // Tüm katkı yapanlara bildirim gönder
        foreach (var participant in participants)
        {
            // Katılım ödülü bildirimi (INotificationService ile)
            await _notificationService.CreateAsync(
                participant.UserId,
                NotificationType.GLOBAL_MISSION_COMPLETED,
                "Dünya görevi bitti! 🎊",
                $"\"{mission.Title}\" tamamlandı! Katılım ödülün: +{PARTICIPATION_POINTS} puan!",
                mission.Id,
                "GLOBAL_MISSION"
            );

            // Ödülü toplandı olarak işaretle
            participant.ClaimReward();
        }

        // Top 3'e ekstra bildirim
        for (int i = 0; i < top3.Count; i++)
        {
            var participant = top3[i];
            var rank = i + 1;
            var bonus = rank switch { 1 => TOP1_BONUS, 2 => TOP2_BONUS, _ => TOP3_BONUS };
            var medal = rank switch { 1 => "🥇", 2 => "🥈", _ => "🥉" };

            await _notificationService.CreateAsync(
                participant.UserId,
                NotificationType.GLOBAL_MISSION_TOP3,
                $"Top {rank}'e girdin! {medal}",
                $"\"{mission.Title}\" görevinde {rank}. sırada bitirdin! Ekstra ödül: +{bonus} puan!",
                mission.Id,
                "GLOBAL_MISSION",
                $"{{\"rank\": {rank}, \"bonus\": {bonus}}}"
            );
        }

        _logger.LogInformation("Finalized mission {MissionId}: {ContributorCount} contributors, Top 3: {Top3Count}", 
            mission.Id, participants.Count, top3.Count);
    }
}

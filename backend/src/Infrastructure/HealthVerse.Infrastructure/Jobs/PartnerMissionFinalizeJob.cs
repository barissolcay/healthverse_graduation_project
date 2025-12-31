using HealthVerse.Contracts.Notifications;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Partner Mission bitiş job'ı.
/// Haftalık çalışır (Pazar 23:55 TR).
/// 
/// - Progress >= TargetValue olan görevleri COMPLETED yapar
/// - PARTNER_COMPLETED bildirimi gönderir
/// - Ödül puanlarını dağıtır
/// </summary>
[DisallowConcurrentExecution]
public sealed class PartnerMissionFinalizeJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly ILogger<PartnerMissionFinalizeJob> _logger;

    // Partner mission başarı ödülü
    private const int COMPLETION_POINTS = 100;

    public PartnerMissionFinalizeJob(
        HealthVerseDbContext dbContext,
        INotificationService notificationService,
        IClock clock,
        ILogger<PartnerMissionFinalizeJob> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _clock = clock;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _clock.UtcNow;
        _logger.LogInformation("PartnerMissionFinalizeJob started at {Time}", now);

        try
        {
            // Aktif partner görevlerini çek
            var activeMissions = await _dbContext.WeeklyPartnerMissions
                .Where(m => m.Status == PartnerMissionStatus.ACTIVE)
                .ToListAsync();

            if (!activeMissions.Any())
            {
                _logger.LogInformation("No active partner missions to finalize");
                return;
            }

            var userIds = activeMissions.SelectMany(m => new[] { m.InitiatorId, m.PartnerId }).Distinct().ToList();
            var users = await _dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username.Value);

            int completedCount = 0;
            int failedCount = 0;

            foreach (var mission in activeMissions)
            {
                // Tamamlandı mı kontrol et
                if (mission.ProgressPercent >= 100)
                {
                    mission.Finish(now);
                    completedCount++;

                    // Her iki tarafa bildirim gönder
                    var initiatorName = users.GetValueOrDefault(mission.InitiatorId, "Partner");
                    var partnerName = users.GetValueOrDefault(mission.PartnerId, "Partner");

                    // Initiator'a bildirim (INotificationService ile)
                    await _notificationService.CreateAsync(
                        mission.InitiatorId,
                        NotificationType.PARTNER_COMPLETED,
                        "Ortak görev tamam! 🎉",
                        $"{partnerName} ile görevi tamamladınız! Ödül: +{COMPLETION_POINTS} puan!",
                        mission.Id,
                        "PARTNER_MISSION"
                    );

                    // Partner'a bildirim (INotificationService ile)
                    await _notificationService.CreateAsync(
                        mission.PartnerId,
                        NotificationType.PARTNER_COMPLETED,
                        "Ortak görev tamam! 🎉",
                        $"{initiatorName} ile görevi tamamladınız! Ödül: +{COMPLETION_POINTS} puan!",
                        mission.Id,
                        "PARTNER_MISSION"
                    );

                    // Puanları ekle
                    var initiator = await _dbContext.Users.FindAsync(mission.InitiatorId);
                    var partner = await _dbContext.Users.FindAsync(mission.PartnerId);
                    initiator?.AddPoints(COMPLETION_POINTS);
                    partner?.AddPoints(COMPLETION_POINTS);
                }
                else
                {
                    // Başarısız - FAILED olarak işaretle
                    mission.Expire(now);
                    failedCount++;
                }
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("PartnerMissionFinalizeJob completed. Completed: {Completed}, Failed: {Failed}", 
                completedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PartnerMissionFinalizeJob failed");
            throw;
        }
    }
}

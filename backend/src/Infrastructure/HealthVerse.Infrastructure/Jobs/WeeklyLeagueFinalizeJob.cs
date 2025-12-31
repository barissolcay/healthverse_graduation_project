using HealthVerse.Competition.Application.Services;
using HealthVerse.Contracts.Notifications;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Haftalık lig finalize job.
/// Her Pazartesi 00:05 TR saatinde çalışır.
/// 
/// 1. Bir önceki haftanın odalarını finalize et (promote/demote)
/// 2. UserPointsHistory kayıtları oluşturulur (FinalizeService tarafından)
/// 3. Bildirimler oluşturulur (bu job tarafından)
/// </summary>
[DisallowConcurrentExecution]
public sealed class WeeklyLeagueFinalizeJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly LeagueFinalizeService _leagueFinalizeService;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly ILogger<WeeklyLeagueFinalizeJob> _logger;

    public WeeklyLeagueFinalizeJob(
        HealthVerseDbContext dbContext,
        LeagueFinalizeService leagueFinalizeService,
        INotificationService notificationService,
        IClock clock,
        ILogger<WeeklyLeagueFinalizeJob> logger)
    {
        _dbContext = dbContext;
        _leagueFinalizeService = leagueFinalizeService;
        _notificationService = notificationService;
        _clock = clock;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _clock.UtcNow;
        _logger.LogInformation("WeeklyLeagueFinalizeJob started at {Time}", now);

        try
        {
            // 1. Bir önceki haftanın WeekId'sini hesapla
            // Job Pazartesi 00:05'te çalışıyor, kapanan hafta Pazar günüydü
            var lastSunday = _clock.TodayTR.AddDays(-1);
            var previousWeekId = WeekId.FromDate(lastSunday);
            
            _logger.LogInformation("Finalizing week: {WeekId}", previousWeekId.Value);

            // 2. Lig finalize işlemini çalıştır (promote/demote + history kayıtları)
            var result = await _leagueFinalizeService.FinalizeWeek(previousWeekId.Value);
            
            _logger.LogInformation(
                "League finalize completed. Rooms: {Rooms}, Promoted: {Promoted}, Demoted: {Demoted}, Stayed: {Stayed}",
                result.ProcessedRooms, result.PromotedUsers, result.DemotedUsers, result.StayedUsers);

            // 3. Terfi/tenzil bildirimleri oluştur
            await CreatePromotionNotifications();

            _logger.LogInformation("WeeklyLeagueFinalizeJob completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WeeklyLeagueFinalizeJob failed");
            throw;
        }
    }

    private async Task CreatePromotionNotifications()
    {
        // Son hafta sonuçlarını çek (son 24 saat içinde oluşturulan)
        var lastWeekResults = await _dbContext.UserPointsHistories
            .Where(h => h.CreatedAt >= _clock.UtcNow.AddDays(-1))
            .ToListAsync();

        foreach (var result in lastWeekResults)
        {
            string notificationType;
            string title;
            string body;

            if (result.Result == "PROMOTED")
            {
                notificationType = NotificationType.LEAGUE_PROMOTED;
                title = "Tebrikler! Liga yükseldin! 🚀";
                body = $"Bu hafta {result.LeagueRank}. sırada bitirdin ve bir üst lige terfi ettin!";
            }
            else if (result.Result == "DEMOTED")
            {
                notificationType = NotificationType.LEAGUE_DEMOTED;
                title = "Lig düştü 😔";
                body = $"Bu hafta {result.LeagueRank}. sırada bitirdin. Yeni haftada geri dönelim!";
            }
            else
            {
                // STAYED - bildirim gönder
                notificationType = NotificationType.LEAGUE_STAYED;
                title = "Ligini korudun!";
                body = $"Bu hafta {result.LeagueRank}. sırada bitirdin. Bu hafta yükselme bölgesine oynayalım!";
            }

            await _notificationService.CreateAsync(
                result.UserId,
                notificationType,
                title,
                body,
                null,
                null,
                default);
        }

        await _dbContext.SaveChangesAsync();
    }
}
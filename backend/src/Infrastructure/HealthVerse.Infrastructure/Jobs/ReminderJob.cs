using HealthVerse.Contracts.Notifications;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Social.Domain.Entities;
using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HealthVerse.Infrastructure.Jobs;

/// <summary>
/// Deadline hatırlatma job'ı.
/// Her saat çalışır.
/// 
/// - Duel: Bitime 24 saat kala → DUEL_ENDING
/// - Partner Mission: Bitime 24 saat kala + %80 altı → PARTNER_ENDING
/// - Global Mission: Bitime 24 saat kala + katkı=0 → GLOBAL_MISSION_ENDING
/// - Task: Bitime 6 saat kala → TASK_EXPIRING
/// - Goal: Bitime 24 saat kala → GOAL_EXPIRING
/// </summary>
[DisallowConcurrentExecution]
public sealed class ReminderJob : IJob
{
    private readonly HealthVerseDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly ILogger<ReminderJob> _logger;

    public ReminderJob(
        HealthVerseDbContext dbContext,
        INotificationService notificationService,
        IClock clock,
        ILogger<ReminderJob> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _clock = clock;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _clock.UtcNow;
        _logger.LogInformation("ReminderJob started at {Time}", now);

        try
        {
            await SendDuelEndingReminders(now);
            await SendPartnerMissionEndingReminders(now);
            await SendGlobalMissionEndingReminders(now);
            await SendTaskExpiringReminders(now);
            await SendGoalExpiringReminders(now);

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("ReminderJob completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReminderJob failed");
            throw;
        }
    }

    /// <summary>
    /// Düello bitiş hatırlatması: EndDate'e 24 saat kala.
    /// </summary>
    private async Task SendDuelEndingReminders(DateTimeOffset now)
    {
        var hoursAheadStart = now.AddHours(23);
        var hoursAheadEnd = now.AddHours(25);

        var endingDuels = await _dbContext.Duels
            .Where(d => d.Status == DuelStatus.ACTIVE &&
                        d.EndDate != null &&
                        d.EndDate >= hoursAheadStart &&
                        d.EndDate <= hoursAheadEnd)
            .ToListAsync();

        // Son 12 saat içinde bu düello için hatırlatma gönderilmiş mi?
        var duelIds = endingDuels.Select(d => d.Id).ToList();
        var recentReminders = await _dbContext.Notifications
            .Where(n => n.Type == NotificationType.DUEL_ENDING &&
                        n.ReferenceId != null &&
                        duelIds.Contains(n.ReferenceId.Value) &&
                        n.CreatedAt >= now.AddHours(-12))
            .Select(n => n.ReferenceId!.Value)
            .ToListAsync();

        // Kullanıcı bilgilerini çek
        var userIds = endingDuels.SelectMany(d => new[] { d.ChallengerId, d.OpponentId }).Distinct().ToList();
        var users = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username.Value);

        foreach (var duel in endingDuels)
        {
            if (recentReminders.Contains(duel.Id)) continue;

            var challengerName = users.GetValueOrDefault(duel.ChallengerId, "Rakip");
            var opponentName = users.GetValueOrDefault(duel.OpponentId, "Rakip");

            // Challenger'a ve Opponent'a bildirim
            await _notificationService.CreateBatchAsync(new[]
            {
                new NotificationCreateRequest(
                    duel.ChallengerId,
                    NotificationType.DUEL_ENDING,
                    "Düello bitiyor! ⏰",
                    $"Son 24 saat! {opponentName} ile düello kapanıyor. Son sprint zamanı!",
                    duel.Id,
                    "DUEL"),
                new NotificationCreateRequest(
                    duel.OpponentId,
                    NotificationType.DUEL_ENDING,
                    "Düello bitiyor! ⏰",
                    $"Son 24 saat! {challengerName} ile düello kapanıyor. Son sprint zamanı!",
                    duel.Id,
                    "DUEL")
            }, default);
        }

        _logger.LogInformation("Sent {Count} duel ending reminders", endingDuels.Count - recentReminders.Count);
    }

    /// <summary>
    /// Partner mission bitiş hatırlatması: Hafta sonuna 24 saat kala + %80 altı.
    /// </summary>
    private async Task SendPartnerMissionEndingReminders(DateTimeOffset now)
    {
        // Mevcut hafta sonuna kadar 24 saat kaldı mı? (Pazar 24:00 TR) 
        var sundayMidnight = GetNextSundayMidnight(now);
        var hoursUntilSunday = (sundayMidnight - now).TotalHours;

        if (hoursUntilSunday > 24 || hoursUntilSunday < 0) return;

        var activeMissions = await _dbContext.WeeklyPartnerMissions
            .Where(m => m.Status == PartnerMissionStatus.ACTIVE && m.ProgressPercent < 80)
            .ToListAsync();

        // Son 12 saat içinde hatırlatma gönderilmiş mi?
        var missionIds = activeMissions.Select(m => m.Id).ToList();
        var recentReminders = await _dbContext.Notifications
            .Where(n => n.Type == NotificationType.PARTNER_ENDING &&
                        n.ReferenceId != null &&
                        missionIds.Contains(n.ReferenceId.Value) &&
                        n.CreatedAt >= now.AddHours(-12))
            .Select(n => n.ReferenceId!.Value)
            .ToListAsync();

        var userIds = activeMissions.SelectMany(m => new[] { m.InitiatorId, m.PartnerId }).Distinct().ToList();
        var users = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username.Value);

        foreach (var mission in activeMissions)
        {
            if (recentReminders.Contains(mission.Id)) continue;

            var initiatorName = users.GetValueOrDefault(mission.InitiatorId, "Partner");
            var partnerName = users.GetValueOrDefault(mission.PartnerId, "Partner");

            // Initiator'a ve Partner'a bildirim
            await _notificationService.CreateBatchAsync(new[]
            {
                new NotificationCreateRequest(
                    mission.InitiatorId,
                    NotificationType.PARTNER_ENDING,
                    "Ortak görev bitiyor! ⏰",
                    $"24 saat kaldı! %{mission.ProgressPercent} tamamlandı. {partnerName} ile bitirin!",
                    mission.Id,
                    "PARTNER_MISSION"),
                new NotificationCreateRequest(
                    mission.PartnerId,
                    NotificationType.PARTNER_ENDING,
                    "Ortak görev bitiyor! ⏰",
                    $"24 saat kaldı! %{mission.ProgressPercent} tamamlandı. {initiatorName} ile bitirin!",
                    mission.Id,
                    "PARTNER_MISSION")
            }, default);
        }

        _logger.LogInformation("Sent {Count} partner mission ending reminders", activeMissions.Count - recentReminders.Count);
    }

    /// <summary>
    /// Global mission bitiş hatırlatması: Bitime 24 saat kala + katkı=0.
    /// </summary>
    private async Task SendGlobalMissionEndingReminders(DateTimeOffset now)
    {
        var hoursAheadStart = now.AddHours(23);
        var hoursAheadEnd = now.AddHours(25);

        var endingMissions = await _dbContext.GlobalMissions
            .Where(m => m.Status == MissionStatus.ACTIVE &&
                        m.EndDate >= hoursAheadStart &&
                        m.EndDate <= hoursAheadEnd)
            .ToListAsync();

        if (!endingMissions.Any()) return;

        var missionIds = endingMissions.Select(m => m.Id).ToList();

        // Katkı=0 olan katılımcılar
        var zeroContributors = await _dbContext.GlobalMissionParticipants
            .Where(p => missionIds.Contains(p.MissionId) && p.ContributionValue == 0)
            .ToListAsync();

        // Son 12 saat içinde hatırlatma gönderilmiş mi?
        var recentReminders = await _dbContext.Notifications
            .Where(n => n.Type == NotificationType.GLOBAL_MISSION_ENDING &&
                        n.CreatedAt >= now.AddHours(-12))
            .Select(n => new { n.UserId, n.ReferenceId })
            .ToListAsync();

        var recentSet = recentReminders
            .Where(r => r.ReferenceId != null)
            .Select(r => (r.UserId, r.ReferenceId!.Value))
            .ToHashSet();

        foreach (var participant in zeroContributors)
        {
            if (recentSet.Contains((participant.UserId, participant.MissionId))) continue;

            var mission = endingMissions.First(m => m.Id == participant.MissionId);

            await _notificationService.CreateAsync(
                participant.UserId,
                NotificationType.GLOBAL_MISSION_ENDING,
                "Dünya görevi bitiyor! 🌍",
                $"\"{mission.Title}\" bitiyor! En az 1 katkı yapmadan ödül alamazsın!",
                mission.Id,
                "GLOBAL_MISSION",
                default);
        }

        _logger.LogInformation("Sent {Count} global mission ending reminders", zeroContributors.Count);
    }

    /// <summary>
    /// Task bitiş hatırlatması: ValidUntil'e 6 saat kala.
    /// </summary>
    private async Task SendTaskExpiringReminders(DateTimeOffset now)
    {
        var hoursAheadStart = now.AddHours(5);
        var hoursAheadEnd = now.AddHours(7);

        var expiringTasks = await _dbContext.UserTasks
            .Where(t => t.Status == UserTaskStatus.ACTIVE &&
                        t.ValidUntil >= hoursAheadStart &&
                        t.ValidUntil <= hoursAheadEnd)
            .ToListAsync();

        // Son 4 saat içinde hatırlatma gönderilmiş mi?
        var taskIds = expiringTasks.Select(t => t.Id).ToList();
        var recentReminders = await _dbContext.Notifications
            .Where(n => n.Type == NotificationType.TASK_EXPIRING &&
                        n.ReferenceId != null &&
                        taskIds.Contains(n.ReferenceId.Value) &&
                        n.CreatedAt >= now.AddHours(-4))
            .Select(n => n.ReferenceId!.Value)
            .ToListAsync();

        // Template bilgilerini çek
        var templateIds = expiringTasks.Select(t => t.TemplateId).Distinct().ToList();
        var templates = await _dbContext.TaskTemplates
            .Where(t => templateIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Title);

        foreach (var task in expiringTasks)
        {
            if (recentReminders.Contains(task.Id)) continue;

            var taskTitle = templates.GetValueOrDefault(task.TemplateId, "Görev");

            await _notificationService.CreateAsync(
                task.UserId,
                NotificationType.TASK_EXPIRING,
                "Görev bitiyor! ⏰",
                $"\"{taskTitle}\" için 6 saat kaldı!",
                task.Id,
                "TASK",
                default);
        }

        _logger.LogInformation("Sent {Count} task expiring reminders", expiringTasks.Count - recentReminders.Count);
    }

    /// <summary>
    /// Goal bitiş hatırlatması: ValidUntil'e 24 saat kala.
    /// </summary>
    private async Task SendGoalExpiringReminders(DateTimeOffset now)
    {
        var hoursAheadStart = now.AddHours(23);
        var hoursAheadEnd = now.AddHours(25);

        var expiringGoals = await _dbContext.UserGoals
            .Where(g => g.CompletedAt == null &&
                        g.ValidUntil >= hoursAheadStart &&
                        g.ValidUntil <= hoursAheadEnd)
            .ToListAsync();

        // Son 12 saat içinde hatırlatma gönderilmiş mi?
        var goalIds = expiringGoals.Select(g => g.Id).ToList();
        var recentReminders = await _dbContext.Notifications
            .Where(n => n.Type == NotificationType.GOAL_EXPIRING &&
                        n.ReferenceId != null &&
                        goalIds.Contains(n.ReferenceId.Value) &&
                        n.CreatedAt >= now.AddHours(-12))
            .Select(n => n.ReferenceId!.Value)
            .ToListAsync();

        foreach (var goal in expiringGoals)
        {
            if (recentReminders.Contains(goal.Id)) continue;

            await _notificationService.CreateAsync(
                goal.UserId,
                NotificationType.GOAL_EXPIRING,
                "Hedefin bitiyor! 🎯",
                $"\"{goal.Title}\" için 24 saat kaldı!",
                goal.Id,
                "GOAL",
                default);
        }

        _logger.LogInformation("Sent {Count} goal expiring reminders", expiringGoals.Count - recentReminders.Count);
    }

    private DateTimeOffset GetNextSundayMidnight(DateTimeOffset now)
    {
        // TR saatine çevir
        var trNow = now.ToOffset(TimeSpan.FromHours(3));
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)trNow.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0) daysUntilSunday = 7; // Eğer bugün pazarsa, gelecek pazar
        return trNow.Date.AddDays(daysUntilSunday);
    }
}

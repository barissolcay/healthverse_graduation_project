using HealthVerse.Contracts.Health;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Missions.Infrastructure.Services;

/// <summary>
/// Updates Weekly Partner Mission progress when health data is synchronized.
/// Matches activities by ActivityType and TargetMetric.
/// Can trigger mission completion if combined target is reached.
/// </summary>
public sealed class PartnerMissionsProgressUpdater : IHealthProgressUpdater
{
    private readonly HealthVerseDbContext _context;
    private readonly IClock _clock;
    private readonly ILogger<PartnerMissionsProgressUpdater> _logger;

    public PartnerMissionsProgressUpdater(
        HealthVerseDbContext context,
        IClock clock,
        ILogger<PartnerMissionsProgressUpdater> logger)
    {
        _context = context;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Partner missions are updated after duels (Order = 50).
    /// </summary>
    public int Order => 50;

    public async Task<HealthProgressResult> UpdateProgressAsync(
        Guid userId,
        IReadOnlyList<HealthActivityData> activities,
        DateOnly logDate,
        CancellationToken ct = default)
    {
        var now = _clock.NowTR;

        // Get all active partner missions where user is a participant
        var activeMissions = await _context.WeeklyPartnerMissions
            .Where(m => m.Status == PartnerMissionStatus.ACTIVE
                && (m.InitiatorId == userId || m.PartnerId == userId))
            .ToListAsync(ct);

        if (activeMissions.Count == 0)
        {
            return new HealthProgressResult
            {
                ModuleName = "PartnerMissions",
                UpdatedCount = 0,
                CompletedCount = 0
            };
        }

        int updatedCount = 0;
        int finishedCount = 0;
        var details = new List<string>();

        foreach (var mission in activeMissions)
        {
            // Find matching activity
            var matchingActivity = FindMatchingActivity(mission, activities);
            if (matchingActivity == null)
                continue;

            // Determine which side the user is on
            bool isInitiator = mission.InitiatorId == userId;
            int previousProgress = isInitiator ? mission.InitiatorProgress : mission.PartnerProgress;

            if (isInitiator)
            {
                mission.UpdateInitiatorProgress(matchingActivity.Value, now);
            }
            else
            {
                mission.UpdatePartnerProgress(matchingActivity.Value, now);
            }

            int newProgress = isInitiator ? mission.InitiatorProgress : mission.PartnerProgress;

            if (newProgress != previousProgress)
            {
                updatedCount++;
                details.Add(
                    $"PartnerMission {mission.Id}: {(isInitiator ? "Initiator" : "Partner")} progress {previousProgress} -> {newProgress}");

                // Check if mission is completed (combined progress reaches target)
                if (mission.IsCompleted && mission.Status == PartnerMissionStatus.ACTIVE)
                {
                    mission.Finish(now);
                    finishedCount++;
                    _logger.LogInformation(
                        "WeeklyPartnerMission {MissionId} completed. Total: {Total}/{Target}",
                        mission.Id, mission.TotalProgress, mission.TargetValue);
                }
            }
        }

        return new HealthProgressResult
        {
            ModuleName = "PartnerMissions",
            UpdatedCount = updatedCount,
            CompletedCount = finishedCount,
            Details = details
        };
    }

    private static HealthActivityData? FindMatchingActivity(
        WeeklyPartnerMission mission,
        IReadOnlyList<HealthActivityData> activities)
    {
        foreach (var activity in activities)
        {
            // Metric must match
            if (!string.Equals(activity.TargetMetric, mission.TargetMetric, StringComparison.OrdinalIgnoreCase))
                continue;

            // If mission has ActivityType, it must match
            if (!string.IsNullOrEmpty(mission.ActivityType))
            {
                if (!string.Equals(activity.ActivityType, mission.ActivityType, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            return activity;
        }

        return null;
    }
}

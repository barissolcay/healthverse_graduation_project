using HealthVerse.Contracts.Health;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Missions.Infrastructure.Services;

/// <summary>
/// Updates Global Mission contributions when health data is synchronized.
/// Creates contribution records and updates the mission's CurrentValue cache.
/// Uses IdempotencyKey to prevent duplicate contributions.
/// </summary>
public sealed class GlobalMissionsProgressUpdater : IHealthProgressUpdater
{
    private readonly HealthVerseDbContext _context;
    private readonly IClock _clock;
    private readonly ILogger<GlobalMissionsProgressUpdater> _logger;

    public GlobalMissionsProgressUpdater(
        HealthVerseDbContext context,
        IClock clock,
        ILogger<GlobalMissionsProgressUpdater> logger)
    {
        _context = context;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Global missions are updated last (Order = 60).
    /// </summary>
    public int Order => 60;

    public async Task<HealthProgressResult> UpdateProgressAsync(
        Guid userId,
        IReadOnlyList<HealthActivityData> activities,
        DateOnly logDate,
        CancellationToken ct = default)
    {
        var now = _clock.NowTR;

        // Get all active global missions
        var activeMissions = await _context.GlobalMissions
            .Where(m => m.Status == MissionStatus.ACTIVE
                && m.StartDate <= now
                && m.EndDate > now)
            .ToListAsync(ct);

        if (activeMissions.Count == 0)
        {
            return new HealthProgressResult
            {
                ModuleName = "GlobalMissions",
                UpdatedCount = 0,
                CompletedCount = 0
            };
        }

        int contributionsAdded = 0;
        int finishedCount = 0;
        var details = new List<string>();

        foreach (var mission in activeMissions)
        {
            // Find matching activity
            var matchingActivity = FindMatchingActivity(mission, activities);
            if (matchingActivity == null)
                continue;

            // Generate idempotency key
            var idempotencyKey = GenerateIdempotencyKey(mission.Id, userId, logDate, matchingActivity);

            // Check if contribution already exists
            var existingContribution = await _context.GlobalMissionContributions
                .FirstOrDefaultAsync(c => c.IdempotencyKey == idempotencyKey, ct);

            if (existingContribution != null)
            {
                _logger.LogDebug(
                    "Duplicate contribution ignored for mission {MissionId}, key: {Key}",
                    mission.Id, idempotencyKey);
                continue;
            }

            // Create contribution
            var contribution = GlobalMissionContribution.Create(
                mission.Id,
                userId,
                matchingActivity.Value,
                idempotencyKey);

            _context.GlobalMissionContributions.Add(contribution);

            // Update mission cache
            long previousValue = mission.CurrentValue;
            mission.AddContribution(matchingActivity.Value);
            long newValue = mission.CurrentValue;

            contributionsAdded++;
            details.Add($"GlobalMission {mission.Id}: +{matchingActivity.Value} ({previousValue} -> {newValue})");

            // Check if mission is now completed
            if (mission.IsCompleted && mission.Status == MissionStatus.ACTIVE)
            {
                mission.Finish();
                finishedCount++;
                _logger.LogInformation(
                    "GlobalMission {MissionId} '{Title}' completed! Target: {Target}",
                    mission.Id, mission.Title, mission.TargetValue);
            }
        }

        return new HealthProgressResult
        {
            ModuleName = "GlobalMissions",
            UpdatedCount = contributionsAdded,
            CompletedCount = finishedCount,
            Details = details
        };
    }

    private static HealthActivityData? FindMatchingActivity(
        GlobalMission mission,
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

    private static string GenerateIdempotencyKey(
        Guid missionId,
        Guid userId,
        DateOnly logDate,
        HealthActivityData activity)
    {
        // Format: MISSION:{MissionId}:{UserId}:{Date}:{ActivityType}:{Metric}
        return $"MISSION:{missionId}:{userId}:{logDate:yyyy-MM-dd}:{activity.ActivityType}:{activity.TargetMetric}";
    }
}

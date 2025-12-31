using HealthVerse.Contracts.Health;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Social.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Social.Infrastructure.Services;

/// <summary>
/// Updates Duel scores when health data is synchronized.
/// Matches activities by ActivityType and TargetMetric.
/// Can trigger duel completion if target is reached.
/// </summary>
public sealed class DuelsProgressUpdater : IHealthProgressUpdater
{
    private readonly HealthVerseDbContext _context;
    private readonly IClock _clock;
    private readonly ILogger<DuelsProgressUpdater> _logger;

    public DuelsProgressUpdater(
        HealthVerseDbContext context,
        IClock clock,
        ILogger<DuelsProgressUpdater> logger)
    {
        _context = context;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Duels are updated after tasks (Order = 40).
    /// </summary>
    public int Order => 40;

    public async Task<HealthProgressResult> UpdateProgressAsync(
        Guid userId,
        IReadOnlyList<HealthActivityData> activities,
        DateOnly logDate,
        CancellationToken ct = default)
    {
        var now = _clock.NowTR;

        // Get all active duels where user is a participant
        var activeDuels = await _context.Duels
            .Where(d => d.Status == DuelStatus.ACTIVE
                && (d.ChallengerId == userId || d.OpponentId == userId))
            .ToListAsync(ct);

        if (activeDuels.Count == 0)
        {
            return new HealthProgressResult
            {
                ModuleName = "Duels",
                UpdatedCount = 0,
                CompletedCount = 0
            };
        }

        int updatedCount = 0;
        int finishedCount = 0;
        var details = new List<string>();

        foreach (var duel in activeDuels)
        {
            // Check if duel has expired
            if (duel.IsExpired(now))
            {
                duel.Finish(now);
                finishedCount++;
                details.Add($"Duel {duel.Id} expired and finished");
                continue;
            }

            // Find matching activity
            var matchingActivity = FindMatchingActivity(duel, activities);
            if (matchingActivity == null)
                continue;

            // Update score based on which side the user is on
            bool isChallenger = duel.ChallengerId == userId;
            int previousScore = isChallenger ? duel.ChallengerScore : duel.OpponentScore;

            if (isChallenger)
            {
                duel.UpdateChallengerScore(matchingActivity.Value, now);
            }
            else
            {
                duel.UpdateOpponentScore(matchingActivity.Value, now);
            }

            int newScore = isChallenger ? duel.ChallengerScore : duel.OpponentScore;

            if (newScore != previousScore)
            {
                updatedCount++;
                details.Add($"Duel {duel.Id}: {(isChallenger ? "Challenger" : "Opponent")} score {previousScore} -> {newScore}");

                // Check if target reached - early finish
                if (newScore >= duel.TargetValue)
                {
                    duel.Finish(now);
                    finishedCount++;
                    _logger.LogInformation(
                        "Duel {DuelId} finished early - {Side} reached target. Result: {Result}",
                        duel.Id, isChallenger ? "Challenger" : "Opponent", duel.Result);
                }
            }
        }

        return new HealthProgressResult
        {
            ModuleName = "Duels",
            UpdatedCount = updatedCount,
            CompletedCount = finishedCount,
            Details = details
        };
    }

    private static HealthActivityData? FindMatchingActivity(
        Duel duel,
        IReadOnlyList<HealthActivityData> activities)
    {
        // Duels always have ActivityType and TargetMetric
        foreach (var activity in activities)
        {
            // Metric must match
            if (!string.Equals(activity.TargetMetric, duel.TargetMetric, StringComparison.OrdinalIgnoreCase))
                continue;

            // ActivityType must match
            if (!string.Equals(activity.ActivityType, duel.ActivityType, StringComparison.OrdinalIgnoreCase))
                continue;

            return activity;
        }

        return null;
    }
}

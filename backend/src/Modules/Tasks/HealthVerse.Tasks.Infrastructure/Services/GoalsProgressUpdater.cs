using HealthVerse.Contracts.Health;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Tasks.Infrastructure.Services;

/// <summary>
/// Updates UserGoal progress when health data is synchronized.
/// Matches activities by ActivityType and TargetMetric.
/// </summary>
public sealed class GoalsProgressUpdater : IHealthProgressUpdater
{
    private readonly HealthVerseDbContext _context;
    private readonly ILogger<GoalsProgressUpdater> _logger;

    public GoalsProgressUpdater(
        HealthVerseDbContext context,
        ILogger<GoalsProgressUpdater> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Goals are updated early (Order = 20).
    /// </summary>
    public int Order => 20;

    public async Task<HealthProgressResult> UpdateProgressAsync(
        Guid userId,
        IReadOnlyList<HealthActivityData> activities,
        DateOnly logDate,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Get all active goals for this user
        var activeGoals = await _context.UserGoals
            .Where(g => g.UserId == userId
                && g.CompletedAt == null
                && g.ValidUntil > now)
            .ToListAsync(ct);

        if (activeGoals.Count == 0)
        {
            return new HealthProgressResult
            {
                ModuleName = "Goals",
                UpdatedCount = 0,
                CompletedCount = 0
            };
        }

        int updatedCount = 0;
        int completedCount = 0;
        var details = new List<string>();

        foreach (var goal in activeGoals)
        {
            // Find matching activity
            var matchingActivity = FindMatchingActivity(goal, activities);
            if (matchingActivity == null)
                continue;

            // Update progress
            int previousValue = goal.CurrentValue;
            bool completed = goal.UpdateProgress(matchingActivity.Value);

            if (goal.CurrentValue != previousValue)
            {
                updatedCount++;
                details.Add($"Goal '{goal.Title}': {previousValue} -> {goal.CurrentValue}");

                if (completed)
                {
                    completedCount++;
                    _logger.LogInformation(
                        "Goal {GoalId} completed for user {UserId}: {Title}",
                        goal.Id, userId, goal.Title);
                }
            }
        }

        return new HealthProgressResult
        {
            ModuleName = "Goals",
            UpdatedCount = updatedCount,
            CompletedCount = completedCount,
            Details = details
        };
    }

    private static HealthActivityData? FindMatchingActivity(
        UserGoal goal,
        IReadOnlyList<HealthActivityData> activities)
    {
        // Goals match by TargetMetric, and optionally by ActivityType
        foreach (var activity in activities)
        {
            // Metric must match
            if (!string.Equals(activity.TargetMetric, goal.TargetMetric, StringComparison.OrdinalIgnoreCase))
                continue;

            // If goal has ActivityType, it must match
            if (!string.IsNullOrEmpty(goal.ActivityType))
            {
                if (string.IsNullOrEmpty(activity.ActivityType))
                    continue;

                if (!string.Equals(activity.ActivityType, goal.ActivityType, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            return activity;
        }

        return null;
    }
}

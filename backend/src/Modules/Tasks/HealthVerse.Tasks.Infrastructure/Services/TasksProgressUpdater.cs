using HealthVerse.Contracts.Health;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Tasks.Infrastructure.Services;

/// <summary>
/// Updates UserTask progress when health data is synchronized.
/// Matches activities by ActivityType and TargetMetric from TaskTemplate.
/// Task completion triggers point rewards.
/// </summary>
public sealed class TasksProgressUpdater : IHealthProgressUpdater
{
    private readonly HealthVerseDbContext _context;
    private readonly ILogger<TasksProgressUpdater> _logger;

    public TasksProgressUpdater(
        HealthVerseDbContext context,
        ILogger<TasksProgressUpdater> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Tasks are updated after goals (Order = 30).
    /// </summary>
    public int Order => 30;

    public async Task<HealthProgressResult> UpdateProgressAsync(
        Guid userId,
        IReadOnlyList<HealthActivityData> activities,
        DateOnly logDate,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Get all active tasks for this user with their templates
        var activeTasks = await _context.UserTasks
            .Where(t => t.UserId == userId && t.Status == UserTaskStatus.ACTIVE)
            .ToListAsync(ct);

        if (activeTasks.Count == 0)
        {
            return new HealthProgressResult
            {
                ModuleName = "Tasks",
                UpdatedCount = 0,
                CompletedCount = 0
            };
        }

        // Get all related templates
        var templateIds = activeTasks.Select(t => t.TemplateId).Distinct().ToList();
        var templates = await _context.TaskTemplates
            .Where(t => templateIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, ct);

        int updatedCount = 0;
        int completedCount = 0;
        int pointsEarned = 0;
        var details = new List<string>();

        foreach (var task in activeTasks)
        {
            if (!templates.TryGetValue(task.TemplateId, out var template))
                continue;

            // Find matching activity
            var matchingActivity = FindMatchingActivity(template, activities);
            if (matchingActivity == null)
                continue;

            // Update progress
            int previousValue = task.CurrentValue;
            bool completed = task.UpdateProgress(matchingActivity.Value, template.TargetValue);

            if (task.CurrentValue != previousValue)
            {
                updatedCount++;
                details.Add($"Task '{template.Title}': {previousValue} -> {task.CurrentValue}/{template.TargetValue}");

                if (completed)
                {
                    completedCount++;
                    pointsEarned += template.RewardPoints;

                    _logger.LogInformation(
                        "Task {TaskId} completed for user {UserId}: {Title} (+{Points} points)",
                        task.Id, userId, template.Title, template.RewardPoints);

                    // Note: Points are added to user in the main handler after all updaters run
                }
            }
        }

        return new HealthProgressResult
        {
            ModuleName = "Tasks",
            UpdatedCount = updatedCount,
            CompletedCount = completedCount,
            PointsEarned = pointsEarned,
            Details = details
        };
    }

    private static HealthActivityData? FindMatchingActivity(
        TaskTemplate template,
        IReadOnlyList<HealthActivityData> activities)
    {
        // Tasks match by TargetMetric, and optionally by ActivityType
        foreach (var activity in activities)
        {
            // Metric must match
            if (!string.Equals(activity.TargetMetric, template.TargetMetric, StringComparison.OrdinalIgnoreCase))
                continue;

            // If template has ActivityType, it must match
            if (!string.IsNullOrEmpty(template.ActivityType))
            {
                if (string.IsNullOrEmpty(activity.ActivityType))
                    continue;

                if (!string.Equals(activity.ActivityType, template.ActivityType, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            return activity;
        }

        return null;
    }
}

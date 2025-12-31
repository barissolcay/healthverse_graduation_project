namespace HealthVerse.Contracts.Health;

/// <summary>
/// Contract interface for updating progress on health-related entities.
/// Each module implements this interface to update its own entities
/// when health data is synchronized.
/// 
/// This follows the hexagonal architecture pattern where:
/// - Interface is defined in Contracts (shared)
/// - Implementation is in each Module's Infrastructure
/// - Gamification module coordinates all updates via this interface
/// </summary>
public interface IHealthProgressUpdater
{
    /// <summary>
    /// The order in which this updater should run.
    /// Lower numbers run first.
    /// Suggested order: Steps(10), Goals(20), Tasks(30), Duels(40), Missions(50)
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Updates progress for entities matching the given health data.
    /// </summary>
    /// <param name="userId">The user whose progress is being updated</param>
    /// <param name="activities">List of health activity data from Flutter</param>
    /// <param name="logDate">The TR date for this sync</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing update details</returns>
    Task<HealthProgressResult> UpdateProgressAsync(
        Guid userId,
        IReadOnlyList<HealthActivityData> activities,
        DateOnly logDate,
        CancellationToken ct = default);
}

/// <summary>
/// Result of a health progress update operation.
/// </summary>
public sealed record HealthProgressResult
{
    /// <summary>
    /// Name of the module that processed the update.
    /// </summary>
    public required string ModuleName { get; init; }

    /// <summary>
    /// Number of entities updated (goals, tasks, duels, etc.)
    /// </summary>
    public int UpdatedCount { get; init; }

    /// <summary>
    /// Number of entities completed as a result of this update.
    /// </summary>
    public int CompletedCount { get; init; }

    /// <summary>
    /// Points earned (if applicable, e.g., from task completion).
    /// </summary>
    public int PointsEarned { get; init; }

    /// <summary>
    /// Optional details about what was updated.
    /// </summary>
    public List<string> Details { get; init; } = [];
}

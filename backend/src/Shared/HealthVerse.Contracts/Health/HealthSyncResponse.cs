namespace HealthVerse.Contracts.Health;

/// <summary>
/// Response DTO for health data synchronization.
/// Contains detailed information about all updates made.
/// </summary>
public sealed class HealthSyncResponse
{
    /// <summary>
    /// Indicates if the sync operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Human-readable message about the operation result.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Points earned from daily steps (3000+ = 0, then +1 per 1000 steps).
    /// </summary>
    public int StepPointsEarned { get; init; }

    /// <summary>
    /// Points earned from completed tasks.
    /// </summary>
    public int TaskPointsEarned { get; init; }

    /// <summary>
    /// Total points earned from this sync.
    /// </summary>
    public int TotalPointsEarned => StepPointsEarned + TaskPointsEarned;

    /// <summary>
    /// User's current total points after this operation.
    /// </summary>
    public long CurrentTotalPoints { get; init; }

    /// <summary>
    /// Total steps synced.
    /// </summary>
    public int TotalSteps { get; init; }

    /// <summary>
    /// Whether the daily streak requirement is met (3000+ steps).
    /// </summary>
    public bool StreakSecured { get; init; }

    /// <summary>
    /// Number of goals updated.
    /// </summary>
    public int GoalsUpdated { get; init; }

    /// <summary>
    /// Number of goals completed.
    /// </summary>
    public int GoalsCompleted { get; init; }

    /// <summary>
    /// Number of tasks updated.
    /// </summary>
    public int TasksUpdated { get; init; }

    /// <summary>
    /// Number of tasks completed.
    /// </summary>
    public int TasksCompleted { get; init; }

    /// <summary>
    /// Number of duels updated.
    /// </summary>
    public int DuelsUpdated { get; init; }

    /// <summary>
    /// Number of duels finished (someone won).
    /// </summary>
    public int DuelsFinished { get; init; }

    /// <summary>
    /// Number of partner missions updated.
    /// </summary>
    public int PartnerMissionsUpdated { get; init; }

    /// <summary>
    /// Number of global missions contributed to.
    /// </summary>
    public int GlobalMissionsContributed { get; init; }

    /// <summary>
    /// Indicates if data was already processed today (idempotency).
    /// </summary>
    public bool AlreadyProcessed { get; init; }

    /// <summary>
    /// Number of activities rejected due to invalid recording method.
    /// </summary>
    public int RejectedActivities { get; init; }
}

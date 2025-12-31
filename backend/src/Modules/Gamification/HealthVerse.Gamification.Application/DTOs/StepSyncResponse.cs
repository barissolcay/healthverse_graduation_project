namespace HealthVerse.Gamification.Application.DTOs;

/// <summary>
/// Response DTO for step synchronization endpoint.
/// This keeps internal domain entities hidden from API consumers.
/// </summary>
public sealed class StepSyncResponse
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
    /// Points earned from this sync operation.
    /// Will be 0 if steps were already processed (idempotency) or below threshold.
    /// </summary>
    public long PointsEarned { get; init; }
    
    /// <summary>
    /// User's current total points after this operation.
    /// Null if user info couldn't be retrieved.
    /// </summary>
    public long? CurrentTotalPoints { get; init; }
    
    /// <summary>
    /// Indicates if this data was already processed (idempotency check).
    /// </summary>
    public bool AlreadyProcessed { get; init; }
}

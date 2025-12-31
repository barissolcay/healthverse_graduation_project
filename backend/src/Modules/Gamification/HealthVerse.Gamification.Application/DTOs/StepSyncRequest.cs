namespace HealthVerse.Gamification.Application.DTOs;

/// <summary>
/// Request DTO for step synchronization endpoint.
/// </summary>
public sealed class StepSyncRequest
{
    /// <summary>
    /// User ID (Firebase UID veya sistem GUID'i).
    /// </summary>
    public Guid UserId { get; init; }
    
    /// <summary>
    /// Total step count for the day.
    /// </summary>
    public int StepCount { get; init; }
    
    /// <summary>
    /// When the steps were recorded (client timestamp).
    /// Server will use its own TR timezone for logDate calculation.
    /// </summary>
    public DateTime HappenedAt { get; init; }
    
    /// <summary>
    /// Source identifier (e.g., "apple_health", "google_fit").
    /// </summary>
    public string SourceId { get; init; } = string.Empty;
}

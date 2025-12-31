namespace HealthVerse.Contracts.Health;

/// <summary>
/// Represents a single health activity data point from Flutter Health package.
/// This DTO is used across all modules for health data synchronization.
/// </summary>
public sealed record HealthActivityData
{
    /// <summary>
    /// Activity type from Flutter Health's WorkoutActivityType.
    /// Examples: RUNNING, WALKING, CYCLING, SWIMMING, BASKETBALL
    /// NULL means general activity (e.g., daily steps from pedometer).
    /// </summary>
    public string? ActivityType { get; init; }

    /// <summary>
    /// Target metric being measured.
    /// STEPS: Step count (COUNT)
    /// DISTANCE: Distance in meters (METER)
    /// CALORIES: Active energy burned (CALORIE)
    /// DURATION: Exercise duration in minutes (MINUTE)
    /// </summary>
    public required string TargetMetric { get; init; }

    /// <summary>
    /// The metric value.
    /// For STEPS: total step count
    /// For DISTANCE: total distance in meters
    /// For CALORIES: total calories burned
    /// For DURATION: total duration in minutes
    /// </summary>
    public required int Value { get; init; }

    /// <summary>
    /// Recording method from Flutter Health.
    /// Only AUTOMATIC entries are accepted, MANUAL and UNKNOWN are rejected.
    /// </summary>
    public RecordingMethod RecordingMethod { get; init; } = RecordingMethod.AUTOMATIC;
}

/// <summary>
/// Recording method constants matching Flutter Health RecordingMethod.
/// </summary>
public enum RecordingMethod
{
    /// <summary>
    /// Data collected automatically by device/sensor.
    /// </summary>
    AUTOMATIC = 0,

    /// <summary>
    /// Data entered manually by user - REJECTED.
    /// </summary>
    MANUAL = 1,

    /// <summary>
    /// Active recording (e.g., workout mode) - Accepted on Android.
    /// </summary>
    ACTIVE = 2,

    /// <summary>
    /// Unknown recording method - REJECTED.
    /// </summary>
    UNKNOWN = 3
}

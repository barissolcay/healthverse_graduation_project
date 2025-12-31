namespace HealthVerse.Contracts.Health;

/// <summary>
/// Standard activity type constants matching Flutter Health WorkoutActivityType.
/// These are the primary activity types supported by HealthVerse.
/// </summary>
public static class ActivityTypes
{
    // Cardio
    public const string RUNNING = "RUNNING";
    public const string WALKING = "WALKING";
    public const string CYCLING = "CYCLING";
    public const string SWIMMING = "SWIMMING";
    public const string HIKING = "HIKING";

    // Sports
    public const string BASKETBALL = "BASKETBALL";
    public const string FOOTBALL = "FOOTBALL";
    public const string TENNIS = "TENNIS";
    public const string VOLLEYBALL = "VOLLEYBALL";
    public const string BADMINTON = "BADMINTON";
    public const string TABLE_TENNIS = "TABLE_TENNIS";

    // Fitness
    public const string YOGA = "YOGA";
    public const string PILATES = "PILATES";
    public const string DANCING = "DANCING";
    public const string ELLIPTICAL = "ELLIPTICAL";
    public const string ROWING = "ROWING";
    public const string STAIR_CLIMBING = "STAIR_CLIMBING";
    public const string HIGH_INTENSITY_INTERVAL_TRAINING = "HIGH_INTENSITY_INTERVAL_TRAINING";

    // Other
    public const string OTHER = "OTHER";

    /// <summary>
    /// Checks if the given activity type is valid.
    /// </summary>
    public static bool IsValid(string? activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            return true; // NULL is valid (general activity)

        return activityType.ToUpperInvariant() switch
        {
            RUNNING or WALKING or CYCLING or SWIMMING or HIKING
            or BASKETBALL or FOOTBALL or TENNIS or VOLLEYBALL
            or BADMINTON or TABLE_TENNIS
            or YOGA or PILATES or DANCING or ELLIPTICAL or ROWING
            or STAIR_CLIMBING or HIGH_INTENSITY_INTERVAL_TRAINING
            or OTHER => true,
            _ => false
        };
    }
}

/// <summary>
/// Standard target metric constants matching Flutter Health data types.
/// </summary>
public static class TargetMetrics
{
    /// <summary>
    /// Step count from pedometer or workout.
    /// Flutter Health: HealthDataType.STEPS
    /// Unit: COUNT
    /// </summary>
    public const string STEPS = "STEPS";

    /// <summary>
    /// Distance traveled in meters.
    /// Flutter Health: HealthDataType.DISTANCE_WALKING_RUNNING, DISTANCE_DELTA
    /// Unit: METER
    /// </summary>
    public const string DISTANCE = "DISTANCE";

    /// <summary>
    /// Active energy burned in calories.
    /// Flutter Health: HealthDataType.ACTIVE_ENERGY_BURNED
    /// Unit: CALORIE
    /// </summary>
    public const string CALORIES = "CALORIES";

    /// <summary>
    /// Exercise/workout duration in minutes.
    /// Flutter Health: Derived from HealthDataType.WORKOUT (dateTo - dateFrom)
    /// Unit: MINUTE
    /// </summary>
    public const string DURATION = "DURATION";

    /// <summary>
    /// Checks if the given metric is valid.
    /// </summary>
    public static bool IsValid(string? metric)
    {
        if (string.IsNullOrWhiteSpace(metric))
            return false;

        return metric.ToUpperInvariant() switch
        {
            STEPS or DISTANCE or CALORIES or DURATION => true,
            _ => false
        };
    }
}

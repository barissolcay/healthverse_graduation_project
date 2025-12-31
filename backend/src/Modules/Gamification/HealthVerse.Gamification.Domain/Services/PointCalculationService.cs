namespace HealthVerse.Gamification.Domain.Services;

/// <summary>
/// Domain service for calculating points from steps.
/// Rule: 3000+ steps to maintain streak, every 1000 steps after 3000 = 1 point.
/// </summary>
public class PointCalculationService
{
    public const int StreakThreshold = 3000;
    public const int PointsPerStep = 1000;

    /// <summary>
    /// Calculates points from daily steps.
    /// Formula: max(0, floor((steps - 3000) / 1000))
    /// Examples:
    /// - 7500 steps → (7500 - 3000) / 1000 = 4 points
    /// - 3500 steps → (3500 - 3000) / 1000 = 0 points
    /// - 3000 steps → 0 points (at threshold, not above)
    /// </summary>
    public int CalculatePointsFromSteps(int dailySteps)
    {
        if (dailySteps <= StreakThreshold)
            return 0;

        return (dailySteps - StreakThreshold) / PointsPerStep;
    }

    /// <summary>
    /// Checks if the step count is enough to maintain streak.
    /// </summary>
    public bool MeetsStreakThreshold(int dailySteps)
    {
        return dailySteps >= StreakThreshold;
    }
}


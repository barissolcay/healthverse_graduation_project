namespace HealthVerse.SharedKernel.Abstractions;

/// <summary>
/// Abstraction for time operations, using Turkey timezone (UTC+3).
/// This enables testing with fixed time and centralizes timezone logic.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC time.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Gets today's date in Turkey timezone (Europe/Istanbul).
    /// </summary>
    DateOnly TodayTR { get; }

    /// <summary>
    /// Gets the current time in Turkey timezone.
    /// </summary>
    DateTimeOffset NowTR { get; }

    /// <summary>
    /// Gets the current ISO week identifier (e.g., "2025-W03").
    /// Based on Turkey timezone.
    /// </summary>
    string CurrentWeekId { get; }

    /// <summary>
    /// Converts a UTC time to Turkey timezone.
    /// </summary>
    DateTimeOffset ToTurkeyTime(DateTimeOffset utcTime);

    /// <summary>
    /// Checks if the current Turkey time is within quiet hours (DND).
    /// Handles overnight ranges (e.g., 22:00 - 09:00).
    /// </summary>
    bool IsWithinQuietHours(TimeOnly start, TimeOnly end);

    /// <summary>
    /// Gets the start of the current week (Monday 00:00 TR).
    /// </summary>
    DateTimeOffset CurrentWeekStart { get; }

    /// <summary>
    /// Gets the end of the current week (next Monday 00:00 TR).
    /// </summary>
    DateTimeOffset CurrentWeekEnd { get; }

    /// <summary>
    /// Gets the end of day for a given Turkey date (23:59:59.999 TR).
    /// </summary>
    DateTimeOffset EndOfDayTR(DateOnly date);
}

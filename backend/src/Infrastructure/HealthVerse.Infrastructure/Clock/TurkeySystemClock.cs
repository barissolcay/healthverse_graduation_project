using HealthVerse.SharedKernel.Abstractions;

namespace HealthVerse.Infrastructure.Clock;

/// <summary>
/// Turkey timezone (Europe/Istanbul, UTC+3) clock implementation.
/// Cross-platform: Uses "Europe/Istanbul" on Linux, "Turkey Standard Time" on Windows.
/// </summary>
public sealed class TurkeySystemClock : IClock
{
    private static readonly TimeZoneInfo TurkeyTimeZone = GetTurkeyTimeZone();

    /// <summary>
    /// Attempts to get Turkey timezone, trying IANA ID first (Linux), then Windows ID.
    /// </summary>
    private static TimeZoneInfo GetTurkeyTimeZone()
    {
        // Try IANA ID first (Linux/macOS)
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback to Windows ID
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
    }

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTimeOffset NowTR => TimeZoneInfo.ConvertTime(UtcNow, TurkeyTimeZone);

    public DateOnly TodayTR => DateOnly.FromDateTime(NowTR.DateTime);

    public string CurrentWeekId
    {
        get
        {
            var today = TodayTR;
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            var weekNumber = calendar.GetWeekOfYear(
                today.ToDateTime(TimeOnly.MinValue),
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            var year = today.Year;
            if (weekNumber == 1 && today.Month == 12)
                year++;
            else if (weekNumber >= 52 && today.Month == 1)
                year--;

            return $"{year}-W{weekNumber:D2}";
        }
    }

    public DateTimeOffset ToTurkeyTime(DateTimeOffset utcTime)
    {
        return TimeZoneInfo.ConvertTime(utcTime, TurkeyTimeZone);
    }

    public bool IsWithinQuietHours(TimeOnly start, TimeOnly end)
    {
        var currentTime = TimeOnly.FromDateTime(NowTR.DateTime);

        // Handle overnight range (e.g., 22:00 - 09:00)
        if (start <= end)
        {
            // Normal range (e.g., 00:00 - 09:00)
            return currentTime >= start && currentTime < end;
        }
        else
        {
            // Overnight range (e.g., 22:00 - 09:00)
            return currentTime >= start || currentTime < end;
        }
    }

    public DateTimeOffset CurrentWeekStart
    {
        get
        {
            var today = NowTR.DateTime;
            var daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var monday = today.Date.AddDays(-daysFromMonday);
            return new DateTimeOffset(monday, TurkeyTimeZone.BaseUtcOffset);
        }
    }

    public DateTimeOffset CurrentWeekEnd => CurrentWeekStart.AddDays(7);

    public DateTimeOffset EndOfDayTR(DateOnly date)
    {
        var endOfDay = date.ToDateTime(new TimeOnly(23, 59, 59, 999));
        return new DateTimeOffset(endOfDay, TurkeyTimeZone.BaseUtcOffset);
    }
}

using System.Globalization;
using System.Text.RegularExpressions;
using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.SharedKernel.ValueObjects;

/// <summary>
/// ISO Week identifier value object.
/// Format: YYYY-Www (e.g., "2025-W03")
/// Based on Turkey timezone.
/// </summary>
public sealed partial class WeekId : ValueObject
{
    private static readonly Regex WeekIdPattern = GenerateWeekIdRegex();

    public string Value { get; }

    private WeekId(string value)
    {
        Value = value;
    }

    public static WeekId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("WeekId.Empty", "WeekId cannot be empty.");

        if (!WeekIdPattern.IsMatch(value))
            throw new DomainException("WeekId.InvalidFormat", "WeekId must be in format YYYY-Www (e.g., 2025-W03).");

        return new WeekId(value);
    }

    public static WeekId FromDate(DateOnly date)
    {
        var calendar = CultureInfo.InvariantCulture.Calendar;
        var weekNumber = calendar.GetWeekOfYear(
            date.ToDateTime(TimeOnly.MinValue),
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);

        // Handle year boundary (week 1 might be in December)
        var year = date.Year;
        if (weekNumber == 1 && date.Month == 12)
            year++;
        else if (weekNumber >= 52 && date.Month == 1)
            year--;

        return new WeekId($"{year}-W{weekNumber:D2}");
    }

    public static WeekId FromDate(DateTimeOffset dateTime, TimeZoneInfo turkeyTimeZone)
    {
        var turkeyTime = TimeZoneInfo.ConvertTime(dateTime, turkeyTimeZone);
        return FromDate(DateOnly.FromDateTime(turkeyTime.DateTime));
    }

    public int Year => int.Parse(Value[..4]);
    public int WeekNumber => int.Parse(Value[6..]);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(WeekId weekId) => weekId.Value;

    [GeneratedRegex(@"^\d{4}-W(0[1-9]|[1-4]\d|5[0-3])$", RegexOptions.Compiled)]
    private static partial Regex GenerateWeekIdRegex();
}

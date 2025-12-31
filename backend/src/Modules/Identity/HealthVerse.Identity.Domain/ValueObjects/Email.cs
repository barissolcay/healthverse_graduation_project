using System.Text.RegularExpressions;
using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Identity.Domain.ValueObjects;

/// <summary>
/// Email value object with validation.
/// </summary>
public sealed partial class Email : ValueObject
{
    public const int MaxLength = 100;
    private static readonly Regex EmailPattern = GenerateEmailRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email.Empty", "Email cannot be empty.");

        var trimmed = value.Trim().ToLowerInvariant();

        if (trimmed.Length > MaxLength)
            throw new DomainException("Email.TooLong", $"Email cannot exceed {MaxLength} characters.");

        if (!EmailPattern.IsMatch(trimmed))
            throw new DomainException("Email.Invalid", "Email format is invalid.");

        return new Email(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex GenerateEmailRegex();
}

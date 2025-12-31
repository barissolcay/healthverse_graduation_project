using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Identity.Domain.ValueObjects;

/// <summary>
/// Username value object with validation.
/// </summary>
public sealed class Username : ValueObject
{
    public const int MinLength = 3;
    public const int MaxLength = 50;

    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Username.Empty", "Username cannot be empty.");

        var trimmed = value.Trim();

        if (trimmed.Length < MinLength)
            throw new DomainException("Username.TooShort", $"Username must be at least {MinLength} characters.");

        if (trimmed.Length > MaxLength)
            throw new DomainException("Username.TooLong", $"Username cannot exceed {MaxLength} characters.");

        if (trimmed != value)
            throw new DomainException("Username.NotTrimmed", "Username cannot have leading or trailing whitespace.");

        return new Username(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant(); // Case-insensitive comparison
    }

    public override string ToString() => Value;

    public static implicit operator string(Username username) => username.Value;
}

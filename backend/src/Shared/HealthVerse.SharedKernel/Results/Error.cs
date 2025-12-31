namespace HealthVerse.SharedKernel.Results;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error Validation(string message) => new("Error.Validation", message);
    public static Error NotFound(string entity) => new("Error.NotFound", $"{entity} was not found.");
    public static Error Conflict(string message) => new("Error.Conflict", message);
    public static Error Forbidden(string message) => new("Error.Forbidden", message);
}

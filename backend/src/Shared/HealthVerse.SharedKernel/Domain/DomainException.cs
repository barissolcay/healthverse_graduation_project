namespace HealthVerse.SharedKernel.Domain;

/// <summary>
/// Exception thrown when a domain rule is violated.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message) : base(message)
    {
        Code = "DOMAIN_ERROR";
    }

    public DomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}

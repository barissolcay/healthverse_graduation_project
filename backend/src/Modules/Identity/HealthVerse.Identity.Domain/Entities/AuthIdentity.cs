using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Identity.Domain.Entities;

/// <summary>
/// Firebase Auth kimliğini kullanıcı profiline eşler.
/// Bir kullanıcının birden fazla auth provider'ı olabilir (Google, Apple, Email).
/// </summary>
public sealed class AuthIdentity : Entity
{
    /// <summary>
    /// Firebase Auth UID (Firebase tarafından atanır).
    /// </summary>
    public string FirebaseUid { get; private init; } = null!;
    
    /// <summary>
    /// HealthVerse User ID.
    /// </summary>
    public Guid UserId { get; private init; }
    
    /// <summary>
    /// Auth provider: GOOGLE, APPLE, EMAIL
    /// </summary>
    public string Provider { get; private init; } = null!;
    
    /// <summary>
    /// Provider'dan gelen email (varsa).
    /// </summary>
    public string? ProviderEmail { get; private init; }
    
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset LastLoginAt { get; private set; }

    private AuthIdentity() { }

    public static AuthIdentity Create(
        string firebaseUid,
        Guid userId,
        string provider,
        string? providerEmail = null)
    {
        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new DomainException("AuthIdentity.InvalidUid", "FirebaseUid cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("AuthIdentity.InvalidUser", "UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(provider))
            throw new DomainException("AuthIdentity.InvalidProvider", "Provider cannot be empty.");

        var now = DateTimeOffset.UtcNow;
        return new AuthIdentity
        {
            Id = Guid.NewGuid(),
            FirebaseUid = firebaseUid,
            UserId = userId,
            Provider = provider.ToUpperInvariant(),
            ProviderEmail = providerEmail,
            CreatedAt = now,
            LastLoginAt = now
        };
    }

    /// <summary>
    /// Son giriş zamanını güncelle.
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Auth provider sabitleri.
/// </summary>
public static class AuthProvider
{
    public const string GOOGLE = "GOOGLE";
    public const string APPLE = "APPLE";
    public const string EMAIL = "EMAIL";
}

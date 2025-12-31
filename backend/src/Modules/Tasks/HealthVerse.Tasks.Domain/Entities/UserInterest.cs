namespace HealthVerse.Tasks.Domain.Entities;

/// <summary>
/// Kullanıcının ilgi alanları.
/// Composite PK: (UserId, ActivityType)
/// Görev atama algoritmasında kullanılır.
/// </summary>
public sealed class UserInterest
{
    public Guid UserId { get; private init; }
    
    /// <summary>
    /// Aktivite tipi: RUNNING, WALKING, CYCLING, SWIMMING vb.
    /// </summary>
    public string ActivityType { get; private init; } = null!;
    
    public DateTimeOffset CreatedAt { get; private init; }

    private UserInterest() { }

    public static UserInterest Create(Guid userId, string activityType)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(activityType))
            throw new ArgumentException("ActivityType cannot be empty.", nameof(activityType));

        return new UserInterest
        {
            UserId = userId,
            ActivityType = activityType.ToUpperInvariant(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

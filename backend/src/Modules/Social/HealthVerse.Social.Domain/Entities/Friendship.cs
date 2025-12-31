namespace HealthVerse.Social.Domain.Entities;

/// <summary>
/// Takip (follow) ilişkisini temsil eder.
/// Composite Primary Key: (FollowerId, FollowingId)
/// Kendini takip etme yasaktır (constraint).
/// Mutual (arkadaşlık) = iki yönlü takip.
/// </summary>
public sealed class Friendship
{
    /// <summary>
    /// Takip eden kullanıcının ID'si.
    /// </summary>
    public Guid FollowerId { get; private init; }

    /// <summary>
    /// Takip edilen kullanıcının ID'si.
    /// </summary>
    public Guid FollowingId { get; private init; }

    /// <summary>
    /// Takip başlangıç zamanı.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private init; }

    private Friendship() { }

    public static Friendship Create(Guid followerId, Guid followingId)
    {
        if (followerId == Guid.Empty)
            throw new ArgumentException("FollowerId cannot be empty.", nameof(followerId));

        if (followingId == Guid.Empty)
            throw new ArgumentException("FollowingId cannot be empty.", nameof(followingId));

        if (followerId == followingId)
            throw new InvalidOperationException("A user cannot follow themselves.");

        return new Friendship
        {
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

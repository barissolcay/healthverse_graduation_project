namespace HealthVerse.Social.Application.Ports;

/// <summary>
/// User repository port for social operations (counter updates).
/// Provides only the operations needed by the Social module.
/// </summary>
public interface ISocialUserRepository
{
    /// <summary>
    /// Check if a user exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Increment the FollowingCount for a user.
    /// </summary>
    Task IncrementFollowingAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Decrement the FollowingCount for a user.
    /// </summary>
    Task DecrementFollowingAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Increment the FollowersCount for a user.
    /// </summary>
    Task IncrementFollowersAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Decrement the FollowersCount for a user.
    /// </summary>
    Task DecrementFollowersAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get user profiles by IDs (for DTO enrichment).
    /// Returns a dictionary mapping UserId to UserSummary.
    /// </summary>
    Task<Dictionary<Guid, UserSummary>> GetProfilesAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
}

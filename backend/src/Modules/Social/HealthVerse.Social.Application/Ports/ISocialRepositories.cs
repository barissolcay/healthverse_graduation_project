using HealthVerse.Social.Domain.Entities;

namespace HealthVerse.Social.Application.Ports;

/// <summary>
/// Friendship (follow) repository port.
/// </summary>
public interface IFriendshipRepository
{
    /// <summary>
    /// Get a specific friendship relation.
    /// </summary>
    Task<Friendship?> GetAsync(Guid followerId, Guid followingId, CancellationToken ct = default);

    /// <summary>
    /// Check if a follow relationship exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid followerId, Guid followingId, CancellationToken ct = default);

    /// <summary>
    /// Check if two users are mutual friends (both follow each other).
    /// </summary>
    Task<bool> IsMutualAsync(Guid userId1, Guid userId2, CancellationToken ct = default);

    /// <summary>
    /// Add a new friendship.
    /// </summary>
    Task AddAsync(Friendship friendship, CancellationToken ct = default);

    /// <summary>
    /// Remove a friendship.
    /// </summary>
    Task RemoveAsync(Friendship friendship, CancellationToken ct = default);

    /// <summary>
    /// Count how many users the specified user is following.
    /// </summary>
    Task<int> CountFollowingAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get paginated list of followers.
    /// </summary>
    Task<Paginated<UserSummary>> GetFollowersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Get paginated list of users being followed.
    /// </summary>
    Task<Paginated<UserSummary>> GetFollowingAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Get paginated list of mutual friends.
    /// </summary>
    Task<Paginated<UserSummary>> GetMutualFriendsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}

/// <summary>
/// User block repository port.
/// </summary>
public interface IUserBlockRepository
{
    /// <summary>
    /// Check if either user has blocked the other.
    /// </summary>
    Task<bool> IsBlockedEitherWayAsync(Guid userId, Guid targetUserId, CancellationToken ct = default);

    /// <summary>
    /// Get a specific block relation.
    /// </summary>
    Task<UserBlock?> GetAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);

    /// <summary>
    /// Add a new block.
    /// </summary>
    Task AddAsync(UserBlock block, CancellationToken ct = default);

    /// <summary>
    /// Remove a block.
    /// </summary>
    Task RemoveAsync(UserBlock block, CancellationToken ct = default);
}

/// <summary>
/// Paginated result wrapper.
/// </summary>
public sealed record Paginated<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// User summary projection for social lists.
/// </summary>
public sealed record UserSummary(Guid UserId, string Username, int AvatarId, long TotalPoints, string CurrentTier, string? SelectedTitleId);

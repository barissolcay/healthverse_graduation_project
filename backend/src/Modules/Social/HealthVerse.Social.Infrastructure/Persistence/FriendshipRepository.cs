using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Social.Application.Ports;
using HealthVerse.Social.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Social.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IFriendshipRepository.
/// </summary>
public sealed class FriendshipRepository : IFriendshipRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public FriendshipRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Friendship?> GetAsync(Guid followerId, Guid followingId, CancellationToken ct = default)
    {
        return await _dbContext.Friendships.FindAsync(new object[] { followerId, followingId }, ct);
    }

    public async Task<bool> ExistsAsync(Guid followerId, Guid followingId, CancellationToken ct = default)
    {
        return await _dbContext.Friendships
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, ct);
    }

    public async Task<bool> IsMutualAsync(Guid userId1, Guid userId2, CancellationToken ct = default)
    {
        // Both directions must exist for mutual friendship
        var forward = await _dbContext.Friendships
            .AnyAsync(f => f.FollowerId == userId1 && f.FollowingId == userId2, ct);
        
        if (!forward) return false;

        var backward = await _dbContext.Friendships
            .AnyAsync(f => f.FollowerId == userId2 && f.FollowingId == userId1, ct);

        return backward;
    }

    public async Task AddAsync(Friendship friendship, CancellationToken ct = default)
    {
        await _dbContext.Friendships.AddAsync(friendship, ct);
    }

    public Task RemoveAsync(Friendship friendship, CancellationToken ct = default)
    {
        _dbContext.Friendships.Remove(friendship);
        return Task.CompletedTask;
    }

    public async Task<int> CountFollowingAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Friendships.CountAsync(f => f.FollowerId == userId, ct);
    }

    public async Task<Paginated<UserSummary>> GetFollowersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbContext.Friendships
            .Where(f => f.FollowingId == userId)
            .Join(_dbContext.Users,
                f => f.FollowerId,
                u => u.Id,
                (f, u) => new UserSummary(u.Id, u.Username.Value, u.AvatarId, u.TotalPoints, u.CurrentTier, u.SelectedTitleId));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Paginated<UserSummary>(items, totalCount, page, pageSize);
    }

    public async Task<Paginated<UserSummary>> GetFollowingAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbContext.Friendships
            .Where(f => f.FollowerId == userId)
            .Join(_dbContext.Users,
                f => f.FollowingId,
                u => u.Id,
                (f, u) => new UserSummary(u.Id, u.Username.Value, u.AvatarId, u.TotalPoints, u.CurrentTier, u.SelectedTitleId));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Paginated<UserSummary>(items, totalCount, page, pageSize);
    }

    public async Task<Paginated<UserSummary>> GetMutualFriendsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        // Mutual = I follow them AND they follow me
        var query = from following in _dbContext.Friendships
                    where following.FollowerId == userId
                    join reverse in _dbContext.Friendships
                        on new { A = following.FollowingId, B = following.FollowerId }
                        equals new { A = reverse.FollowerId, B = reverse.FollowingId }
                    join user in _dbContext.Users
                        on following.FollowingId equals user.Id
                    select new UserSummary(user.Id, user.Username.Value, user.AvatarId, user.TotalPoints, user.CurrentTier, user.SelectedTitleId);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Paginated<UserSummary>(items, totalCount, page, pageSize);
    }
}

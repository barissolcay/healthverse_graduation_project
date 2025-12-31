using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Missions.Infrastructure.Services;

public sealed class FriendshipService : IFriendshipService
{
    private readonly HealthVerseDbContext _dbContext;

    public FriendshipService(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsMutualFriendAsync(Guid userId1, Guid userId2, CancellationToken ct = default)
    {
        var follows1to2 = await _dbContext.Friendships
            .AnyAsync(f => f.FollowerId == userId1 && f.FollowingId == userId2, ct);
        var follows2to1 = await _dbContext.Friendships
            .AnyAsync(f => f.FollowerId == userId2 && f.FollowingId == userId1, ct);
        return follows1to2 && follows2to1;
    }

    public async Task<List<Guid>> GetMutualFriendIdsAsync(Guid userId, CancellationToken ct = default)
    {
        var following = await _dbContext.Friendships
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync(ct);

        var mutuals = await _dbContext.Friendships
            .Where(f => f.FollowingId == userId && following.Contains(f.FollowerId))
            .Select(f => f.FollowerId)
            .ToListAsync(ct);

        return mutuals;
    }
}

// Note: NotificationService removed - now using central INotificationService from HealthVerse.Notifications.Application.Ports

public sealed class MissionsUserService : IMissionsUserService
{
    private readonly HealthVerseDbContext _dbContext;

    public MissionsUserService(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dictionary<Guid, (string Username, int AvatarId)>> GetUsersAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => (u.Username.Value, u.AvatarId), ct);
    }

    public async Task<(string Username, int AvatarId)?> GetUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Username.Value, u.AvatarId })
            .FirstOrDefaultAsync(ct);

        if (user is null) return null;
        return (user.Value, user.AvatarId);
    }
}

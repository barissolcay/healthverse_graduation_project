using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Social.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Social.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of ISocialUserRepository.
/// Provides user operations needed by the Social module.
/// </summary>
public sealed class SocialUserRepository : ISocialUserRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public SocialUserRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Users.AnyAsync(u => u.Id == userId, ct);
    }

    public async Task IncrementFollowingAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { userId }, ct);
        user?.IncrementFollowingCount();
    }

    public async Task DecrementFollowingAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { userId }, ct);
        user?.DecrementFollowingCount();
    }

    public async Task IncrementFollowersAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { userId }, ct);
        user?.IncrementFollowersCount();
    }

    public async Task DecrementFollowersAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { userId }, ct);
        user?.DecrementFollowersCount();
    }

    public async Task<Dictionary<Guid, UserSummary>> GetProfilesAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, UserSummary>();

        var users = await _dbContext.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new UserSummary(u.Id, u.Username.Value, u.AvatarId, u.TotalPoints, u.CurrentTier, u.SelectedTitleId))
            .ToListAsync(ct);

        return users.ToDictionary(u => u.UserId);
    }
}

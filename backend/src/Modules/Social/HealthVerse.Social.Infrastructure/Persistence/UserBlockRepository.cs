using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Social.Application.Ports;
using HealthVerse.Social.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Social.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IUserBlockRepository.
/// </summary>
public sealed class UserBlockRepository : IUserBlockRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public UserBlockRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsBlockedEitherWayAsync(Guid userId, Guid targetUserId, CancellationToken ct = default)
    {
        return await _dbContext.UserBlocks
            .AnyAsync(b => 
                (b.BlockerId == userId && b.BlockedId == targetUserId) ||
                (b.BlockerId == targetUserId && b.BlockedId == userId), ct);
    }

    public async Task<UserBlock?> GetAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default)
    {
        return await _dbContext.UserBlocks.FindAsync(new object[] { blockerId, blockedId }, ct);
    }

    public async Task AddAsync(UserBlock block, CancellationToken ct = default)
    {
        await _dbContext.UserBlocks.AddAsync(block, ct);
    }

    public Task RemoveAsync(UserBlock block, CancellationToken ct = default)
    {
        _dbContext.UserBlocks.Remove(block);
        return Task.CompletedTask;
    }
}

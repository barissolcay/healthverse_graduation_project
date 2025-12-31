using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Social.Application.Ports;
using HealthVerse.Social.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Social.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IDuelRepository.
/// </summary>
public sealed class DuelRepository : IDuelRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public DuelRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasActiveOrPendingBetweenAsync(Guid userA, Guid userB, CancellationToken ct = default)
    {
        return await _dbContext.Duels
            .AnyAsync(d =>
                ((d.ChallengerId == userA && d.OpponentId == userB) ||
                 (d.ChallengerId == userB && d.OpponentId == userA)) &&
                (d.Status == DuelStatus.WAITING || d.Status == DuelStatus.ACTIVE), ct);
    }

    public async Task AddAsync(Duel duel, CancellationToken ct = default)
    {
        await _dbContext.Duels.AddAsync(duel, ct);
    }

    public async Task<Duel?> GetByIdAsync(Guid duelId, CancellationToken ct = default)
    {
        return await _dbContext.Duels.FindAsync(new object[] { duelId }, ct);
    }

    public async Task<List<Duel>> GetPendingIncomingAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Duels
            .Where(d => d.OpponentId == userId && d.Status == DuelStatus.WAITING)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Duel>> GetPendingOutgoingAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Duels
            .Where(d => d.ChallengerId == userId && d.Status == DuelStatus.WAITING)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Duel>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Duels
            .Where(d => (d.ChallengerId == userId || d.OpponentId == userId) && d.Status == DuelStatus.ACTIVE)
            .OrderBy(d => d.EndDate)
            .ToListAsync(ct);
    }

    public async Task<List<Duel>> GetHistoryByUserAsync(Guid userId, int limit, CancellationToken ct = default)
    {
        return await _dbContext.Duels
            .Where(d => 
                (d.ChallengerId == userId || d.OpponentId == userId) &&
                (d.Status == DuelStatus.FINISHED || d.Status == DuelStatus.REJECTED || d.Status == DuelStatus.EXPIRED))
            .OrderByDescending(d => d.UpdatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<int> ExpireOldDuelsAsync(DateTimeOffset now, CancellationToken ct = default)
    {
        var cutoff = now.AddHours(-24);
        var oldDuels = await _dbContext.Duels
            .Where(d => d.Status == DuelStatus.WAITING && d.CreatedAt < cutoff)
            .ToListAsync(ct);

        foreach (var duel in oldDuels)
        {
            duel.Expire(now);
        }

        return oldDuels.Count;
    }

    public async Task<int> FinishExpiredDuelsAsync(DateTimeOffset now, CancellationToken ct = default)
    {
        var expiredDuels = await _dbContext.Duels
            .Where(d => d.Status == DuelStatus.ACTIVE && d.EndDate <= now)
            .ToListAsync(ct);

        foreach (var duel in expiredDuels)
        {
            duel.Finish(now);
        }

        return expiredDuels.Count;
    }
}

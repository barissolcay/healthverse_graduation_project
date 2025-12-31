using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Gamification.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IPointTransactionRepository.
/// </summary>
public sealed class PointTransactionRepository : IPointTransactionRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public PointTransactionRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default)
    {
        return await _dbContext.PointTransactions
            .AnyAsync(t => t.IdempotencyKey.Value == key, ct);
    }

    public async Task<PointTransaction?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default)
    {
        return await _dbContext.PointTransactions
            .FirstOrDefaultAsync(t => t.IdempotencyKey.Value == key, ct);
    }

    public async Task AddAsync(PointTransaction transaction, CancellationToken ct = default)
    {
        await _dbContext.PointTransactions.AddAsync(transaction, ct);
    }

    public async Task<List<PointTransaction>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken ct = default)
    {
        return await _dbContext.PointTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }
}

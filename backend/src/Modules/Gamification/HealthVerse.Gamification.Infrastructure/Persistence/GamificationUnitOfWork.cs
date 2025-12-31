using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Infrastructure.Persistence;

namespace HealthVerse.Gamification.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IGamificationUnitOfWork.
/// </summary>
public sealed class GamificationUnitOfWork : IGamificationUnitOfWork
{
    private readonly HealthVerseDbContext _dbContext;

    public GamificationUnitOfWork(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}

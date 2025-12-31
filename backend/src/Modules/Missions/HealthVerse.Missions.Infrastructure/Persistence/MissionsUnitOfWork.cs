using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Application.Ports;

namespace HealthVerse.Missions.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IMissionsUnitOfWork.
/// </summary>
public sealed class MissionsUnitOfWork : IMissionsUnitOfWork
{
    private readonly HealthVerseDbContext _dbContext;

    public MissionsUnitOfWork(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}

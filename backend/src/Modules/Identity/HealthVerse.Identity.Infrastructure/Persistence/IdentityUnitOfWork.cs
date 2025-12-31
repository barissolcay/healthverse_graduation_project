using HealthVerse.Identity.Application.Ports;
using HealthVerse.Infrastructure.Persistence;

namespace HealthVerse.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IIdentityUnitOfWork.
/// </summary>
public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly HealthVerseDbContext _dbContext;

    public IdentityUnitOfWork(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}

using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Social.Application.Ports;

namespace HealthVerse.Social.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of ISocialUnitOfWork.
/// </summary>
public sealed class SocialUnitOfWork : ISocialUnitOfWork
{
    private readonly HealthVerseDbContext _dbContext;

    public SocialUnitOfWork(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}

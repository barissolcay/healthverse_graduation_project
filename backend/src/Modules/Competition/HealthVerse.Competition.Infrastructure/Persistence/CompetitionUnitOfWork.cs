using HealthVerse.Competition.Application.Ports;
using HealthVerse.Infrastructure.Persistence;

namespace HealthVerse.Competition.Infrastructure.Persistence;

public sealed class CompetitionUnitOfWork : ICompetitionUnitOfWork
{
    private readonly HealthVerseDbContext _dbContext;

    public CompetitionUnitOfWork(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}

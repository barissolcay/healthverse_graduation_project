using HealthVerse.Competition.Application.Ports;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Competition.Infrastructure.Persistence;

public sealed class LeagueConfigRepository : ILeagueConfigRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public LeagueConfigRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LeagueConfig?> GetByTierNameAsync(string tierName, CancellationToken ct = default)
    {
        return await _dbContext.LeagueConfigs
            .FirstOrDefaultAsync(c => c.TierName == tierName, ct);
    }

    public async Task<List<LeagueConfig>> GetAllOrderedAsync(CancellationToken ct = default)
    {
        return await _dbContext.LeagueConfigs
            .OrderBy(c => c.TierOrder)
            .ToListAsync(ct);
    }

    public async Task<LeagueConfig?> GetNextByOrderAsync(int tierOrder, CancellationToken ct = default)
    {
        return await _dbContext.LeagueConfigs
            .Where(c => c.TierOrder == tierOrder + 1)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<LeagueConfig?> GetPrevByOrderAsync(int tierOrder, CancellationToken ct = default)
    {
        return await _dbContext.LeagueConfigs
            .Where(c => c.TierOrder == tierOrder - 1)
            .FirstOrDefaultAsync(ct);
    }
}

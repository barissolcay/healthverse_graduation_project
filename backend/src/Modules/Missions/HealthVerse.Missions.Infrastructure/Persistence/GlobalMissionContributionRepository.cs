using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Missions.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IGlobalMissionContributionRepository.
/// </summary>
public sealed class GlobalMissionContributionRepository : IGlobalMissionContributionRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public GlobalMissionContributionRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default)
    {
        return await _dbContext.GlobalMissionContributions
            .AnyAsync(c => c.IdempotencyKey == key, ct);
    }

    public async Task AddAsync(GlobalMissionContribution contribution, CancellationToken ct = default)
    {
        await _dbContext.GlobalMissionContributions.AddAsync(contribution, ct);
    }
}

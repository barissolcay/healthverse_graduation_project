using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Missions.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IGlobalMissionRepository.
/// </summary>
public sealed class GlobalMissionRepository : IGlobalMissionRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public GlobalMissionRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GlobalMission>> GetActiveAsync(DateTimeOffset now, CancellationToken ct = default)
    {
        return await _dbContext.GlobalMissions
            .Where(m => m.Status == MissionStatus.ACTIVE && m.StartDate <= now && m.EndDate > now)
            .OrderBy(m => m.EndDate)
            .ToListAsync(ct);
    }

    public async Task<List<GlobalMission>> GetHistoryAsync(DateTimeOffset now, int limit, CancellationToken ct = default)
    {
        return await _dbContext.GlobalMissions
            .Where(m => m.Status == MissionStatus.FINISHED || m.EndDate <= now)
            .OrderByDescending(m => m.EndDate)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<GlobalMission?> GetByIdAsync(Guid missionId, CancellationToken ct = default)
    {
        return await _dbContext.GlobalMissions.FindAsync(new object[] { missionId }, ct);
    }

    public Task UpdateCacheAsync(GlobalMission mission, CancellationToken ct = default)
    {
        // EF Core will track changes automatically
        // This method exists in case we need to do explicit updates
        _dbContext.GlobalMissions.Update(mission);
        return Task.CompletedTask;
    }
}

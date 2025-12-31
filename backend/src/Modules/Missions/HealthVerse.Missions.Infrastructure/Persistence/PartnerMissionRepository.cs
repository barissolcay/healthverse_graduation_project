using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Missions.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IPartnerMissionRepository.
/// </summary>
public sealed class PartnerMissionRepository : IPartnerMissionRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public PartnerMissionRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WeeklyPartnerMission?> GetByIdAsync(Guid missionId, CancellationToken ct = default)
    {
        return await _dbContext.WeeklyPartnerMissions.FindAsync(new object[] { missionId }, ct);
    }

    public async Task<WeeklyPartnerMission?> GetActiveByUserAsync(string weekId, Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.WeeklyPartnerMissions
            .FirstOrDefaultAsync(m =>
                m.WeekId == weekId &&
                (m.InitiatorId == userId || m.PartnerId == userId) &&
                m.Status == PartnerMissionStatus.ACTIVE, ct);
    }

    public async Task<List<WeeklyPartnerMission>> GetHistoryByUserAsync(Guid userId, int limit, CancellationToken ct = default)
    {
        return await _dbContext.WeeklyPartnerMissions
            .Where(m =>
                (m.InitiatorId == userId || m.PartnerId == userId) &&
                m.Status != PartnerMissionStatus.ACTIVE)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task AddAsync(WeeklyPartnerMission mission, CancellationToken ct = default)
    {
        await _dbContext.WeeklyPartnerMissions.AddAsync(mission, ct);
    }
}

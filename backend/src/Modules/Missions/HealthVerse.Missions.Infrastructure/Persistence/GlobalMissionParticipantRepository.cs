using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Missions.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IGlobalMissionParticipantRepository.
/// </summary>
public sealed class GlobalMissionParticipantRepository : IGlobalMissionParticipantRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public GlobalMissionParticipantRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GlobalMissionParticipant?> GetAsync(Guid missionId, Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.GlobalMissionParticipants
            .FirstOrDefaultAsync(p => p.MissionId == missionId && p.UserId == userId, ct);
    }

    public async Task AddAsync(GlobalMissionParticipant participant, CancellationToken ct = default)
    {
        await _dbContext.GlobalMissionParticipants.AddAsync(participant, ct);
    }

    public async Task<Dictionary<Guid, GlobalMissionParticipant>> GetByUserAsync(Guid userId, IEnumerable<Guid> missionIds, CancellationToken ct = default)
    {
        var ids = missionIds.ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, GlobalMissionParticipant>();

        var participants = await _dbContext.GlobalMissionParticipants
            .Where(p => p.UserId == userId && ids.Contains(p.MissionId))
            .ToListAsync(ct);

        return participants.ToDictionary(p => p.MissionId);
    }

    public async Task<List<GlobalMissionParticipant>> GetTopContributorsAsync(Guid missionId, int take, CancellationToken ct = default)
    {
        return await _dbContext.GlobalMissionParticipants
            .Where(p => p.MissionId == missionId)
            .OrderByDescending(p => p.ContributionValue)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(Guid missionId, CancellationToken ct = default)
    {
        return await _dbContext.GlobalMissionParticipants
            .CountAsync(p => p.MissionId == missionId, ct);
    }
}

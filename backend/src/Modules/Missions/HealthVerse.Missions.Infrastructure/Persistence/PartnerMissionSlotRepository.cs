using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Missions.Application.Ports;
using HealthVerse.Missions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Missions.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IPartnerMissionSlotRepository.
/// </summary>
public sealed class PartnerMissionSlotRepository : IPartnerMissionSlotRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public PartnerMissionSlotRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsUserBusyAsync(string weekId, Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.WeeklyPartnerMissionSlots
            .AnyAsync(s => s.WeekId == weekId && s.UserId == userId, ct);
    }

    public async Task AddAsync(WeeklyPartnerMissionSlot slot, CancellationToken ct = default)
    {
        await _dbContext.WeeklyPartnerMissionSlots.AddAsync(slot, ct);
    }
}

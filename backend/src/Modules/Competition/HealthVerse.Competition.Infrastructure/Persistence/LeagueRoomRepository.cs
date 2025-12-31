using HealthVerse.Competition.Application.Ports;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Competition.Infrastructure.Persistence;

public sealed class LeagueRoomRepository : ILeagueRoomRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public LeagueRoomRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<LeagueRoom>> GetUnprocessedByWeekAsync(WeekId weekId, CancellationToken ct = default)
    {
        return await _dbContext.LeagueRooms
            .Where(r => r.WeekId.Value == weekId.Value && !r.IsProcessed)
            .ToListAsync(ct);
    }

    public async Task<LeagueRoom?> GetByIdAsync(Guid roomId, CancellationToken ct = default)
    {
        return await _dbContext.LeagueRooms.FindAsync(new object[] { roomId }, ct);
    }

    public async Task<LeagueRoom?> FindAvailableForTierAsync(WeekId weekId, string tier, int maxRoomSize, CancellationToken ct = default)
    {
        return await _dbContext.LeagueRooms
            .Where(r => r.WeekId.Value == weekId.Value
                     && r.Tier == tier
                     && r.UserCount < maxRoomSize
                     && !r.IsProcessed)
            .OrderBy(r => r.UserCount)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(LeagueRoom room, CancellationToken ct = default)
    {
        await _dbContext.LeagueRooms.AddAsync(room, ct);
    }

    public async Task IncrementUserCountAsync(Guid roomId, CancellationToken ct = default)
    {
        var room = await _dbContext.LeagueRooms.FindAsync(new object[] { roomId }, ct);
        room?.IncrementUserCount();
    }
}

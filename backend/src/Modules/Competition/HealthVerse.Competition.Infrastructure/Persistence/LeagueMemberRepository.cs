using HealthVerse.Competition.Application.Ports;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Competition.Infrastructure.Persistence;

public sealed class LeagueMemberRepository : ILeagueMemberRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public LeagueMemberRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LeagueMember?> GetMembershipByUserAndWeekAsync(Guid userId, WeekId weekId, CancellationToken ct = default)
    {
        return await _dbContext.LeagueMembers
            .FirstOrDefaultAsync(m => m.UserId == userId && m.WeekId.Value == weekId.Value, ct);
    }

    public async Task<List<LeagueMember>> GetMembersByRoomOrderedAsync(Guid roomId, CancellationToken ct = default)
    {
        return await _dbContext.LeagueMembers
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.PointsInRoom)
            .ThenBy(m => m.JoinedAt)
            .ToListAsync(ct);
    }

    public async Task<int> CountByRoomAsync(Guid roomId, CancellationToken ct = default)
    {
        return await _dbContext.LeagueMembers
            .CountAsync(m => m.RoomId == roomId, ct);
    }

    public async Task<int> GetRankForUserAsync(Guid roomId, int userPoints, CancellationToken ct = default)
    {
        var higherCount = await _dbContext.LeagueMembers
            .CountAsync(m => m.RoomId == roomId && m.PointsInRoom > userPoints, ct);
        return higherCount + 1;
    }

    public async Task AddAsync(LeagueMember member, CancellationToken ct = default)
    {
        await _dbContext.LeagueMembers.AddAsync(member, ct);
    }
}

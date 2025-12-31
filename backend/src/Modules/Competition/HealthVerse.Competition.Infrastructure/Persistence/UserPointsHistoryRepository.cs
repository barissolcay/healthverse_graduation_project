using HealthVerse.Competition.Application.Ports;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Competition.Infrastructure.Persistence;

public sealed class UserPointsHistoryRepository : IUserPointsHistoryRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public UserPointsHistoryRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddWeeklyHistoryAsync(UserPointsHistory history, CancellationToken ct = default)
    {
        await _dbContext.UserPointsHistories.AddAsync(history, ct);
    }

    public async Task<List<UserPointsHistory>> GetWeeklyHistoryAsync(Guid userId, int limit, CancellationToken ct = default)
    {
        return await _dbContext.UserPointsHistories
            .Where(h => h.UserId == userId && h.PeriodType == "WEEKLY")
            .OrderByDescending(h => h.PeriodId)
            .Take(limit)
            .ToListAsync(ct);
    }
}

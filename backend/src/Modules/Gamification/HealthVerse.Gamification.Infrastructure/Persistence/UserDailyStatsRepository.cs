using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Gamification.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IUserDailyStatsRepository.
/// </summary>
public sealed class UserDailyStatsRepository : IUserDailyStatsRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public UserDailyStatsRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDailyStats?> GetByDateAsync(Guid userId, DateOnly date, CancellationToken ct = default)
    {
        return await _dbContext.UserDailyStats
            .FirstOrDefaultAsync(s => s.UserId == userId && s.LogDate == date, ct);
    }

    public async Task<List<UserDailyStats>> GetByRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
    {
        return await _dbContext.UserDailyStats
            .Where(s => s.UserId == userId && s.LogDate >= startDate && s.LogDate <= endDate)
            .OrderBy(s => s.LogDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(UserDailyStats stats, CancellationToken ct = default)
    {
        await _dbContext.UserDailyStats.AddAsync(stats, ct);
    }

    public async Task<List<(Guid UserId, long TotalPoints)>> GetTopUsersByDateRangeAsync(DateOnly startDate, DateOnly endDate, int limit, CancellationToken ct = default)
    {
        var result = await _dbContext.UserDailyStats
            .Where(s => s.LogDate >= startDate && s.LogDate <= endDate)
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, TotalPoints = (long)g.Sum(x => x.DailyPoints) })
            .OrderByDescending(x => x.TotalPoints)
            .Take(limit)
            .ToListAsync(ct);

        return result.Select(x => (x.UserId, x.TotalPoints)).ToList();
    }
}

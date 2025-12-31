using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Tasks.Application.Ports;
using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Tasks.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IUserGoalRepository.
/// </summary>
public sealed class UserGoalRepository : IUserGoalRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public UserGoalRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserGoal goal, CancellationToken ct = default)
    {
        await _dbContext.UserGoals.AddAsync(goal, ct);
    }

    public async Task<List<UserGoal>> GetActiveByUserAsync(Guid userId, DateTimeOffset now, CancellationToken ct = default)
    {
        return await _dbContext.UserGoals
            .Where(g => g.UserId == userId && 
                   g.CompletedAt == null && 
                   g.ValidUntil > now)
            .OrderBy(g => g.ValidUntil)
            .ToListAsync(ct);
    }

    public async Task<List<UserGoal>> GetCompletedByUserAsync(Guid userId, int limit, CancellationToken ct = default)
    {
        return await _dbContext.UserGoals
            .Where(g => g.UserId == userId && g.CompletedAt != null)
            .OrderByDescending(g => g.CompletedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<UserGoal?> GetByIdAsync(Guid goalId, Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId, ct);
    }

    public Task RemoveAsync(UserGoal goal, CancellationToken ct = default)
    {
        _dbContext.UserGoals.Remove(goal);
        return Task.CompletedTask;
    }
}

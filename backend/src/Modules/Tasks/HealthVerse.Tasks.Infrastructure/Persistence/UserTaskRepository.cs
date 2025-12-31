using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Tasks.Application.Ports;
using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Tasks.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IUserTaskRepository.
/// </summary>
public sealed class UserTaskRepository : IUserTaskRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public UserTaskRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserTask>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserTasks
            .Where(t => t.UserId == userId && t.Status == UserTaskStatus.ACTIVE)
            .OrderBy(t => t.ValidUntil)
            .ToListAsync(ct);
    }

    public async Task<List<UserTask>> GetCompletedByUserAsync(Guid userId, int limit, CancellationToken ct = default)
    {
        return await _dbContext.UserTasks
            .Where(t => t.UserId == userId && 
                   (t.Status == UserTaskStatus.COMPLETED || t.Status == UserTaskStatus.REWARD_CLAIMED))
            .OrderByDescending(t => t.CompletedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<UserTask?> GetByIdAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserTasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId, ct);
    }

    public async Task<List<UserTask>> GetExpiredAsync(Guid userId, DateTimeOffset now, CancellationToken ct = default)
    {
        return await _dbContext.UserTasks
            .Where(t => t.UserId == userId && 
                   t.Status == UserTaskStatus.ACTIVE && 
                   t.ValidUntil <= now)
            .ToListAsync(ct);
    }

    public async Task AddAsync(UserTask task, CancellationToken ct = default)
    {
        await _dbContext.UserTasks.AddAsync(task, ct);
    }
}

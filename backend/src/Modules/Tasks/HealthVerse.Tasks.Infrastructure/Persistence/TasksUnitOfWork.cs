using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Tasks.Application.Ports;

namespace HealthVerse.Tasks.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of ITasksUnitOfWork.
/// </summary>
public sealed class TasksUnitOfWork : ITasksUnitOfWork
{
    private readonly HealthVerseDbContext _dbContext;

    public TasksUnitOfWork(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}

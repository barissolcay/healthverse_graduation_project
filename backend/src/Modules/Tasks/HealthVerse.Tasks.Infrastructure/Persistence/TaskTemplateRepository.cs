using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Tasks.Application.Ports;
using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Tasks.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of ITaskTemplateRepository.
/// </summary>
public sealed class TaskTemplateRepository : ITaskTemplateRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public TaskTemplateRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dictionary<Guid, TaskTemplate>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
            return new Dictionary<Guid, TaskTemplate>();

        var templates = await _dbContext.TaskTemplates
            .Where(t => idList.Contains(t.Id))
            .ToListAsync(ct);

        return templates.ToDictionary(t => t.Id);
    }

    public async Task<List<TaskTemplate>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var query = _dbContext.TaskTemplates.AsQueryable();
        
        if (activeOnly)
            query = query.Where(t => t.IsActive);

        return await query.OrderBy(t => t.Category).ThenBy(t => t.Title).ToListAsync(ct);
    }

    public async Task<TaskTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.TaskTemplates.FindAsync(new object[] { id }, ct);
    }
}

namespace HealthVerse.Tasks.Application.Ports;

/// <summary>
/// Unit of Work interface for Tasks module.
/// </summary>
public interface ITasksUnitOfWork
{
    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

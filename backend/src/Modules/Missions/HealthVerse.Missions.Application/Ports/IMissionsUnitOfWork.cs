namespace HealthVerse.Missions.Application.Ports;

/// <summary>
/// Unit of Work interface for Missions module.
/// </summary>
public interface IMissionsUnitOfWork
{
    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

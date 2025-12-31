namespace HealthVerse.Notifications.Application.Ports;

/// <summary>
/// Unit of Work interface for Notifications module.
/// </summary>
public interface INotificationsUnitOfWork
{
    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

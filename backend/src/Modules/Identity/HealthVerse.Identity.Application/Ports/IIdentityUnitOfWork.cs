namespace HealthVerse.Identity.Application.Ports;

/// <summary>
/// Unit of Work interface for Identity module.
/// </summary>
public interface IIdentityUnitOfWork
{
    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

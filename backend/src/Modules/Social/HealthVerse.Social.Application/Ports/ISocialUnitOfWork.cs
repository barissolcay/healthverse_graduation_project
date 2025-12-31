namespace HealthVerse.Social.Application.Ports;

/// <summary>
/// Unit of Work interface for Social module.
/// Provides transactional commit for all social operations.
/// </summary>
public interface ISocialUnitOfWork
{
    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

namespace HealthVerse.Gamification.Application.Ports;

/// <summary>
/// Unit of Work interface for Gamification module.
/// </summary>
public interface IGamificationUnitOfWork
{
    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

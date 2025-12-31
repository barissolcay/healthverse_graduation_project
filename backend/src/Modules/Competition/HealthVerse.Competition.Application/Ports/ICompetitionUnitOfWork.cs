namespace HealthVerse.Competition.Application.Ports;

/// <summary>
/// Competition module unit of work for transactional saves.
/// </summary>
public interface ICompetitionUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

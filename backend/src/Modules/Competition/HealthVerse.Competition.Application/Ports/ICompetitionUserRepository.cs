using HealthVerse.Identity.Domain.Entities;

namespace HealthVerse.Competition.Application.Ports;

/// <summary>
/// Minimal user repository port for Competition module needs.
/// </summary>
public interface ICompetitionUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<Dictionary<Guid, User>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
}

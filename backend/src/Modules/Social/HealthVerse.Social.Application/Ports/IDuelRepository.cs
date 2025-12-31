using HealthVerse.Social.Domain.Entities;

namespace HealthVerse.Social.Application.Ports;

/// <summary>
/// Duel repository port.
/// </summary>
public interface IDuelRepository
{
    /// <summary>
    /// Check if there's an active or pending duel between two users.
    /// </summary>
    Task<bool> HasActiveOrPendingBetweenAsync(Guid userA, Guid userB, CancellationToken ct = default);

    /// <summary>
    /// Add a new duel.
    /// </summary>
    Task AddAsync(Duel duel, CancellationToken ct = default);

    /// <summary>
    /// Get duel by ID.
    /// </summary>
    Task<Duel?> GetByIdAsync(Guid duelId, CancellationToken ct = default);

    /// <summary>
    /// Get pending incoming duel requests for a user.
    /// </summary>
    Task<List<Duel>> GetPendingIncomingAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get pending outgoing duel requests from a user.
    /// </summary>
    Task<List<Duel>> GetPendingOutgoingAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get active duels for a user.
    /// </summary>
    Task<List<Duel>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get duel history for a user.
    /// </summary>
    Task<List<Duel>> GetHistoryByUserAsync(Guid userId, int limit, CancellationToken ct = default);

    /// <summary>
    /// Expire old duel invitations (WAITING > 24 hours).
    /// Returns the count of expired duels.
    /// </summary>
    Task<int> ExpireOldDuelsAsync(DateTimeOffset now, CancellationToken ct = default);

    /// <summary>
    /// Finish expired active duels (EndDate passed).
    /// Returns the count of finished duels.
    /// </summary>
    Task<int> FinishExpiredDuelsAsync(DateTimeOffset now, CancellationToken ct = default);
}

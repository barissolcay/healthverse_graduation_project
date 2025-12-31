using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;

namespace HealthVerse.Identity.Application.Ports;

/// <summary>
/// User repository port.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get user by ID.
    /// </summary>
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get users by IDs.
    /// </summary>
    Task<Dictionary<Guid, User>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);

    /// <summary>
    /// Get user by username.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Check if username is already taken.
    /// </summary>
    Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Add a new user.
    /// </summary>
    Task AddAsync(User user, CancellationToken ct = default);
}

/// <summary>
/// Auth identity repository port.
/// </summary>
public interface IAuthIdentityRepository
{
    /// <summary>
    /// Get auth identity by Firebase UID.
    /// </summary>
    Task<AuthIdentity?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken ct = default);

    /// <summary>
    /// Get auth identity by provider email (for dev login).
    /// </summary>
    Task<AuthIdentity?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Get all auth identities for a user.
    /// </summary>
    Task<List<AuthIdentity>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Add a new auth identity.
    /// </summary>
    Task AddAsync(AuthIdentity authIdentity, CancellationToken ct = default);
}

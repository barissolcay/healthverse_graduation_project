using HealthVerse.Gamification.Domain.Entities;

namespace HealthVerse.Gamification.Application.Ports;

/// <summary>
/// Point transaction repository port.
/// </summary>
public interface IPointTransactionRepository
{
    /// <summary>
    /// Check if a transaction with the given idempotency key exists.
    /// </summary>
    Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Get transaction by idempotency key.
    /// </summary>
    Task<PointTransaction?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Add a new transaction.
    /// </summary>
    Task AddAsync(PointTransaction transaction, CancellationToken ct = default);

    /// <summary>
    /// Get recent transactions for a user.
    /// </summary>
    Task<List<PointTransaction>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken ct = default);
}

/// <summary>
/// User daily stats repository port.
/// </summary>
public interface IUserDailyStatsRepository
{
    /// <summary>
    /// Get stats for a specific day.
    /// </summary>
    Task<UserDailyStats?> GetByDateAsync(Guid userId, DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Get stats for a date range.
    /// </summary>
    Task<List<UserDailyStats>> GetByRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default);

    /// <summary>
    /// Add new daily stats.
    /// </summary>
    Task AddAsync(UserDailyStats stats, CancellationToken ct = default);

    /// <summary>
    /// Get top users by points for a specific date range.
    /// Returns list of (UserId, TotalPoints).
    /// </summary>
    Task<List<(Guid UserId, long TotalPoints)>> GetTopUsersByDateRangeAsync(DateOnly startDate, DateOnly endDate, int limit, CancellationToken ct = default);
}

/// <summary>
/// Milestone repository port.
/// </summary>
public interface IMilestoneRepository
{
    /// <summary>
    /// Get all milestone definitions (rewards).
    /// </summary>
    Task<List<MilestoneReward>> GetAllDefinitionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get active milestone definitions.
    /// </summary>
    Task<List<MilestoneReward>> GetActiveDefinitionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get milestone by ID.
    /// </summary>
    Task<MilestoneReward?> GetDefinitionByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Get user progress for a specific milestone.
    /// </summary>
    Task<UserMilestone?> GetUserProgressAsync(Guid userId, Guid milestoneId, CancellationToken ct = default);

    /// <summary>
    /// Get all user milestones.
    /// </summary>
    Task<List<UserMilestone>> GetUserMilestonesAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Add user milestone progress.
    /// </summary>
    Task AddUserMilestoneAsync(UserMilestone userMilestone, CancellationToken ct = default);
}

using HealthVerse.Tasks.Domain.Entities;

namespace HealthVerse.Tasks.Application.Ports;

/// <summary>
/// User task repository port.
/// </summary>
public interface IUserTaskRepository
{
    /// <summary>
    /// Get active tasks for a user.
    /// </summary>
    Task<List<UserTask>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get completed tasks for a user (with limit).
    /// </summary>
    Task<List<UserTask>> GetCompletedByUserAsync(Guid userId, int limit, CancellationToken ct = default);

    /// <summary>
    /// Get a specific task by ID for a user.
    /// </summary>
    Task<UserTask?> GetByIdAsync(Guid taskId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get expired tasks for a user.
    /// </summary>
    Task<List<UserTask>> GetExpiredAsync(Guid userId, DateTimeOffset now, CancellationToken ct = default);

    /// <summary>
    /// Add a new user task.
    /// </summary>
    Task AddAsync(UserTask task, CancellationToken ct = default);
}

/// <summary>
/// Task template repository port.
/// </summary>
public interface ITaskTemplateRepository
{
    /// <summary>
    /// Get templates by their IDs.
    /// </summary>
    Task<Dictionary<Guid, TaskTemplate>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Get all templates (optionally only active ones).
    /// </summary>
    Task<List<TaskTemplate>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);

    /// <summary>
    /// Get a single template by ID.
    /// </summary>
    Task<TaskTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// User goal repository port.
/// </summary>
public interface IUserGoalRepository
{
    /// <summary>
    /// Add a new user goal.
    /// </summary>
    Task AddAsync(UserGoal goal, CancellationToken ct = default);

    /// <summary>
    /// Get active goals for a user.
    /// </summary>
    Task<List<UserGoal>> GetActiveByUserAsync(Guid userId, DateTimeOffset now, CancellationToken ct = default);

    /// <summary>
    /// Get completed goals for a user (with limit).
    /// </summary>
    Task<List<UserGoal>> GetCompletedByUserAsync(Guid userId, int limit, CancellationToken ct = default);

    /// <summary>
    /// Get a specific goal by ID for a user.
    /// </summary>
    Task<UserGoal?> GetByIdAsync(Guid goalId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Remove a user goal.
    /// </summary>
    Task RemoveAsync(UserGoal goal, CancellationToken ct = default);
}

/// <summary>
/// User interest repository port.
/// </summary>
public interface IUserInterestRepository
{
    /// <summary>
    /// Get activity types for a user.
    /// </summary>
    Task<List<string>> GetActivityTypesAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Replace all activity types for a user.
    /// </summary>
    Task ReplaceAsync(Guid userId, IEnumerable<string> activityTypes, CancellationToken ct = default);
}

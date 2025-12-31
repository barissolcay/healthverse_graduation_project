namespace HealthVerse.Contracts.Notifications;

/// <summary>
/// Contract interface for notification creation across modules.
/// This is the cross-module contract - implementation lives in Notifications.Infrastructure.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a notification and schedules it for push delivery.
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="type">Notification type (from NotificationType constants)</param>
    /// <param name="title">Notification title</param>
    /// <param name="body">Notification body/message</param>
    /// <param name="referenceId">Optional reference entity ID (DuelId, MissionId, etc.)</param>
    /// <param name="referenceType">Optional reference entity type (DUEL, MISSION, etc.)</param>
    /// <param name="data">Optional JSON data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The ID of the created notification</returns>
    Task<Guid> CreateAsync(
        Guid userId,
        string type,
        string title,
        string body,
        Guid? referenceId = null,
        string? referenceType = null,
        string? data = null,
        CancellationToken ct = default);

    /// <summary>
    /// Creates multiple notifications and schedules them for push delivery.
    /// Useful for batch operations like weekly summaries.
    /// </summary>
    /// <param name="requests">Collection of notification creation requests</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of created notification IDs</returns>
    Task<List<Guid>> CreateBatchAsync(
        IEnumerable<NotificationCreateRequest> requests,
        CancellationToken ct = default);
}

/// <summary>
/// Request object for batch notification creation.
/// </summary>
public sealed record NotificationCreateRequest(
    Guid UserId,
    string Type,
    string Title,
    string Body,
    Guid? ReferenceId = null,
    string? ReferenceType = null,
    string? Data = null);

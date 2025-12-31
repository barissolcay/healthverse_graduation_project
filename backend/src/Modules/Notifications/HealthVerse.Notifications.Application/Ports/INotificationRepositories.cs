using HealthVerse.Notifications.Domain.Entities;

namespace HealthVerse.Notifications.Application.Ports;

/// <summary>
/// Notification repository port.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Get notification by ID.
    /// </summary>
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct = default);

    /// <summary>
    /// Get recent notifications for a user.
    /// </summary>
    Task<List<Notification>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken ct = default);

    /// <summary>
    /// Get unread count for a user.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Add a new notification.
    /// </summary>
    Task AddAsync(Notification notification, CancellationToken ct = default);

    /// <summary>
    /// Get paginated notifications for a user.
    /// </summary>
    Task<List<Notification>> GetPaginatedByUserAsync(Guid userId, int page, int pageSize, bool unreadOnly, CancellationToken ct = default);

    /// <summary>
    /// Get all unread notifications for a user.
    /// </summary>
    Task<List<Notification>> GetUnreadByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get notifications by IDs.
    /// </summary>
    Task<List<Notification>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Count notifications with filters.
    /// </summary>
    Task<int> CountAsync(Guid userId, bool unreadOnly, CancellationToken ct = default);
}

/// <summary>
/// User device repository port.
/// </summary>
public interface IUserDeviceRepository
{
    /// <summary>
    /// Get device by token.
    /// </summary>
    Task<UserDevice?> GetByTokenAsync(string pushToken, CancellationToken ct = default);

    /// <summary>
    /// Get all devices for a user.
    /// </summary>
    Task<List<UserDevice>> GetByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get active devices for a user (for push notifications).
    /// </summary>
    Task<List<UserDevice>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Add a new device.
    /// </summary>
    Task AddAsync(UserDevice device, CancellationToken ct = default);

    /// <summary>
    /// Remove a device.
    /// </summary>
    Task RemoveAsync(UserDevice device, CancellationToken ct = default);

    /// <summary>
    /// Disable a device (invalid token).
    /// </summary>
    Task DisableAsync(Guid deviceId, CancellationToken ct = default);
}

/// <summary>
/// Notification delivery repository port for outbox pattern.
/// </summary>
public interface INotificationDeliveryRepository
{
    /// <summary>
    /// Get deliveries ready to send (Pending + ScheduledAt <= now).
    /// </summary>
    Task<List<NotificationDelivery>> GetReadyToSendAsync(DateTimeOffset now, int take, CancellationToken ct = default);

    /// <summary>
    /// Get delivery by ID.
    /// </summary>
    Task<NotificationDelivery?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Add a new delivery.
    /// </summary>
    Task AddAsync(NotificationDelivery delivery, CancellationToken ct = default);

    /// <summary>
    /// Update an existing delivery.
    /// </summary>
    Task UpdateAsync(NotificationDelivery delivery, CancellationToken ct = default);

    /// <summary>
    /// Get pending deliveries for a notification.
    /// </summary>
    Task<List<NotificationDelivery>> GetPendingByNotificationAsync(Guid notificationId, CancellationToken ct = default);
}

/// <summary>
/// User notification preference repository port.
/// Manages user's notification preferences per category.
/// </summary>
public interface IUserNotificationPreferenceRepository
{
    /// <summary>
    /// Get preference for a user and category.
    /// Returns null if no preference exists (use default policy).
    /// </summary>
    Task<UserNotificationPreference?> GetAsync(Guid userId, NotificationCategory category, CancellationToken ct = default);

    /// <summary>
    /// Get all preferences for a user.
    /// </summary>
    Task<List<UserNotificationPreference>> GetAllByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get preferences for multiple users and a category (batch operation).
    /// Useful for bulk notification decisions.
    /// </summary>
    Task<Dictionary<Guid, UserNotificationPreference>> GetByUsersAndCategoryAsync(
        IEnumerable<Guid> userIds,
        NotificationCategory category,
        CancellationToken ct = default);

    /// <summary>
    /// Add a new preference.
    /// </summary>
    Task AddAsync(UserNotificationPreference preference, CancellationToken ct = default);

    /// <summary>
    /// Update an existing preference.
    /// </summary>
    Task UpdateAsync(UserNotificationPreference preference, CancellationToken ct = default);

    /// <summary>
    /// Upsert a preference (add if not exists, update if exists).
    /// </summary>
    Task UpsertAsync(UserNotificationPreference preference, CancellationToken ct = default);
}

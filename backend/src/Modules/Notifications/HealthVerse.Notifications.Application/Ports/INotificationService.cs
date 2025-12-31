// ===============================================================================
// DEPRECATED: This file is kept for backward compatibility during Phase 6 migration.
// The canonical contract is now in HealthVerse.Contracts.Notifications.
// All new code should use: using HealthVerse.Contracts.Notifications;
// ===============================================================================

using HealthVerse.Notifications.Domain.Entities;

namespace HealthVerse.Notifications.Application.Ports;

/// <summary>
/// Internal interface extending the contract with domain entity returns.
/// This is used by Notifications module internally.
/// External modules should use INotificationService from HealthVerse.Contracts.
/// </summary>
public interface INotificationServiceInternal : Contracts.Notifications.INotificationService
{
    /// <summary>
    /// Creates a notification and returns the full entity (internal use only).
    /// </summary>
    Task<Notification> CreateAndReturnAsync(
        Guid userId,
        string type,
        string title,
        string body,
        Guid? referenceId = null,
        string? referenceType = null,
        string? data = null,
        CancellationToken ct = default);

    /// <summary>
    /// Creates multiple notifications and returns the full entities (internal use only).
    /// </summary>
    Task<List<Notification>> CreateBatchAndReturnAsync(
        IEnumerable<Contracts.Notifications.NotificationCreateRequest> requests,
        CancellationToken ct = default);
}

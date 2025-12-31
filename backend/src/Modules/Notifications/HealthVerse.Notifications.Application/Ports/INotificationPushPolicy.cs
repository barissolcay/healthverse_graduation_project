using HealthVerse.Notifications.Domain.Entities;

namespace HealthVerse.Notifications.Application.Ports;

/// <summary>
/// Push notification policy port.
/// Decides whether a notification should be sent as push based on:
/// - Category policy (is this category push-worthy?)
/// - User preference (did user disable this category?)
/// - Quiet hours (is it within user's quiet hours?)
/// </summary>
public interface INotificationPushPolicy
{
    /// <summary>
    /// Determines if a push notification should be sent for a single user.
    /// </summary>
    /// <param name="userId">Target user ID</param>
    /// <param name="notificationType">Notification type string (e.g., STREAK_FROZEN)</param>
    /// <param name="currentTimeUtc">Current UTC time for quiet hours check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Push decision result</returns>
    Task<PushDecision> ShouldSendPushAsync(
        Guid userId,
        string notificationType,
        DateTimeOffset currentTimeUtc,
        CancellationToken ct = default);

    /// <summary>
    /// Determines push decisions for multiple users (batch operation).
    /// More efficient than calling ShouldSendPushAsync for each user.
    /// </summary>
    /// <param name="userIds">Target user IDs</param>
    /// <param name="notificationType">Notification type string</param>
    /// <param name="currentTimeUtc">Current UTC time for quiet hours check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Dictionary of userId → PushDecision</returns>
    Task<Dictionary<Guid, PushDecision>> ShouldSendPushBatchAsync(
        IEnumerable<Guid> userIds,
        string notificationType,
        DateTimeOffset currentTimeUtc,
        CancellationToken ct = default);
}

/// <summary>
/// Result of push policy evaluation.
/// </summary>
public sealed record PushDecision
{
    /// <summary>
    /// Whether push should be sent.
    /// </summary>
    public bool ShouldSend { get; init; }

    /// <summary>
    /// Reason for the decision (for logging/debugging).
    /// </summary>
    public PushDecisionReason Reason { get; init; }

    /// <summary>
    /// If push is delayed due to quiet hours, when to send.
    /// Null if ShouldSend is true or push is completely blocked.
    /// </summary>
    public DateTimeOffset? ScheduledAt { get; init; }

    public static PushDecision Send() => new()
    {
        ShouldSend = true,
        Reason = PushDecisionReason.Allowed
    };

    public static PushDecision BlockByCategory() => new()
    {
        ShouldSend = false,
        Reason = PushDecisionReason.CategoryDisabledByDefault
    };

    public static PushDecision BlockByUserPreference() => new()
    {
        ShouldSend = false,
        Reason = PushDecisionReason.DisabledByUser
    };

    public static PushDecision DelayForQuietHours(DateTimeOffset scheduledAt) => new()
    {
        ShouldSend = false,
        Reason = PushDecisionReason.QuietHours,
        ScheduledAt = scheduledAt
    };
}

/// <summary>
/// Reason for push decision.
/// </summary>
public enum PushDecisionReason
{
    /// <summary>
    /// Push is allowed to be sent.
    /// </summary>
    Allowed,

    /// <summary>
    /// Category is disabled by default policy (e.g., Goal, Milestone).
    /// </summary>
    CategoryDisabledByDefault,

    /// <summary>
    /// User explicitly disabled this category.
    /// </summary>
    DisabledByUser,

    /// <summary>
    /// Currently in quiet hours, push will be scheduled for later.
    /// </summary>
    QuietHours
}

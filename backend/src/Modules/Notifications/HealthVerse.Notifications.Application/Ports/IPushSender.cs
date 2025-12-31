namespace HealthVerse.Notifications.Application.Ports;

/// <summary>
/// Push notification sender port.
/// Abstracts the push notification provider (FCM, APNs, etc.)
/// </summary>
public interface IPushSender
{
    /// <summary>
    /// Send a push notification to a device.
    /// </summary>
    /// <param name="deviceToken">The device push token.</param>
    /// <param name="title">Notification title.</param>
    /// <param name="body">Notification body.</param>
    /// <param name="data">Optional data payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success/failure.</returns>
    Task<PushResult> SendAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default);

    /// <summary>
    /// Send push notification to multiple devices.
    /// </summary>
    Task<List<PushResult>> SendMultipleAsync(
        IEnumerable<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default);
}

/// <summary>
/// Result of a push notification send attempt.
/// </summary>
public record PushResult(
    bool Success,
    string? MessageId,
    string? Error,
    bool IsInvalidToken = false);

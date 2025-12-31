using FirebaseAdmin.Messaging;
using HealthVerse.Notifications.Application.Ports;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Notifications.Infrastructure.Push;

/// <summary>
/// Firebase Cloud Messaging (FCM) implementation of IPushSender.
/// </summary>
public sealed class FirebasePushSender : IPushSender
{
    private readonly ILogger<FirebasePushSender> _logger;

    public FirebasePushSender(ILogger<FirebasePushSender> logger)
    {
        _logger = logger;
    }

    public async Task<PushResult> SendAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message, ct);
            
            _logger.LogInformation("Push notification sent successfully. MessageId: {MessageId}", messageId);
            
            return new PushResult(Success: true, MessageId: messageId, Error: null);
        }
        catch (FirebaseMessagingException ex) when (IsInvalidTokenError(ex))
        {
            _logger.LogWarning("Invalid device token: {Token}. Error: {Error}", 
                deviceToken[..Math.Min(20, deviceToken.Length)] + "...", ex.Message);
            
            return new PushResult(Success: false, MessageId: null, Error: ex.Message, IsInvalidToken: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to token: {Token}", 
                deviceToken[..Math.Min(20, deviceToken.Length)] + "...");
            
            return new PushResult(Success: false, MessageId: null, Error: ex.Message);
        }
    }

    public async Task<List<PushResult>> SendMultipleAsync(
        IEnumerable<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        var results = new List<PushResult>();
        
        foreach (var token in deviceTokens)
        {
            var result = await SendAsync(token, title, body, data, ct);
            results.Add(result);
        }
        
        return results;
    }

    private static bool IsInvalidTokenError(FirebaseMessagingException ex)
    {
        return ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
               ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument;
    }
}

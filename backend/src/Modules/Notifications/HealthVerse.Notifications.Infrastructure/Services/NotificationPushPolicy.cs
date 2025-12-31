using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Application.Services;
using HealthVerse.Notifications.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HealthVerse.Notifications.Infrastructure.Services;

/// <summary>
/// Implementation of INotificationPushPolicy.
/// Evaluates push decisions based on category defaults and user preferences.
/// </summary>
public sealed class NotificationPushPolicy : INotificationPushPolicy
{
    private readonly IUserNotificationPreferenceRepository _preferenceRepo;
    private readonly ILogger<NotificationPushPolicy> _logger;

    public NotificationPushPolicy(
        IUserNotificationPreferenceRepository preferenceRepo,
        ILogger<NotificationPushPolicy> logger)
    {
        _preferenceRepo = preferenceRepo;
        _logger = logger;
    }

    public async Task<PushDecision> ShouldSendPushAsync(
        Guid userId,
        string notificationType,
        DateTimeOffset currentTimeUtc,
        CancellationToken ct = default)
    {
        var category = NotificationTypeCategoryMapping.GetCategory(notificationType);
        var defaultEnabled = NotificationTypeCategoryMapping.GetDefaultPushEnabled(category);

        // 1. Get user preference (if exists)
        var preference = await _preferenceRepo.GetAsync(userId, category, ct);

        // 2. Determine if push is enabled
        bool pushEnabled;
        if (preference != null)
        {
            // User has explicit preference
            pushEnabled = preference.PushEnabled;
        }
        else
        {
            // Use category default
            pushEnabled = defaultEnabled;
        }

        // 3. If push is disabled, return appropriate reason
        if (!pushEnabled)
        {
            var reason = preference != null
                ? PushDecisionReason.DisabledByUser
                : PushDecisionReason.CategoryDisabledByDefault;

            _logger.LogDebug(
                "Push blocked for user {UserId}, type {Type}, category {Category}, reason: {Reason}",
                userId, notificationType, category, reason);

            return new PushDecision
            {
                ShouldSend = false,
                Reason = reason
            };
        }

        // 4. Check quiet hours (if user has preference with quiet hours)
        if (preference != null && preference.QuietHoursStart.HasValue)
        {
            var currentTime = TimeOnly.FromDateTime(currentTimeUtc.UtcDateTime);
            
            if (preference.IsInQuietHours(currentTime))
            {
                // Calculate when quiet hours end
                var scheduledAt = CalculateQuietHoursEnd(currentTimeUtc, preference.QuietHoursEnd!.Value);
                
                _logger.LogDebug(
                    "Push delayed for user {UserId} due to quiet hours. Scheduled at {ScheduledAt}",
                    userId, scheduledAt);

                return PushDecision.DelayForQuietHours(scheduledAt);
            }
        }

        // 5. Push allowed
        return PushDecision.Send();
    }

    public async Task<Dictionary<Guid, PushDecision>> ShouldSendPushBatchAsync(
        IEnumerable<Guid> userIds,
        string notificationType,
        DateTimeOffset currentTimeUtc,
        CancellationToken ct = default)
    {
        var userIdList = userIds.ToList();
        var category = NotificationTypeCategoryMapping.GetCategory(notificationType);
        var defaultEnabled = NotificationTypeCategoryMapping.GetDefaultPushEnabled(category);

        // Batch fetch preferences for all users
        var preferences = await _preferenceRepo.GetByUsersAndCategoryAsync(userIdList, category, ct);

        var results = new Dictionary<Guid, PushDecision>(userIdList.Count);
        var currentTime = TimeOnly.FromDateTime(currentTimeUtc.UtcDateTime);

        foreach (var userId in userIdList)
        {
            preferences.TryGetValue(userId, out var preference);

            // Determine if push is enabled
            bool pushEnabled = preference?.PushEnabled ?? defaultEnabled;

            if (!pushEnabled)
            {
                var reason = preference != null
                    ? PushDecisionReason.DisabledByUser
                    : PushDecisionReason.CategoryDisabledByDefault;

                results[userId] = new PushDecision
                {
                    ShouldSend = false,
                    Reason = reason
                };
                continue;
            }

            // Check quiet hours
            if (preference != null && preference.QuietHoursStart.HasValue)
            {
                if (preference.IsInQuietHours(currentTime))
                {
                    var scheduledAt = CalculateQuietHoursEnd(currentTimeUtc, preference.QuietHoursEnd!.Value);
                    results[userId] = PushDecision.DelayForQuietHours(scheduledAt);
                    continue;
                }
            }

            // Push allowed
            results[userId] = PushDecision.Send();
        }

        _logger.LogDebug(
            "Batch push decision for {UserCount} users, type {Type}: {AllowedCount} allowed, {BlockedCount} blocked",
            userIdList.Count,
            notificationType,
            results.Count(r => r.Value.ShouldSend),
            results.Count(r => !r.Value.ShouldSend));

        return results;
    }

    /// <summary>
    /// Calculate when quiet hours end (next occurrence of end time).
    /// </summary>
    private static DateTimeOffset CalculateQuietHoursEnd(DateTimeOffset currentTime, TimeOnly quietHoursEnd)
    {
        var today = currentTime.Date;
        var endDateTime = today.Add(quietHoursEnd.ToTimeSpan());

        // If end time is before current time, it means quiet hours end tomorrow
        if (endDateTime <= currentTime.DateTime)
        {
            endDateTime = endDateTime.AddDays(1);
        }

        return new DateTimeOffset(endDateTime, TimeSpan.Zero);
    }
}

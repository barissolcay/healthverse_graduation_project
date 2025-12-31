using HealthVerse.Notifications.Application.DTOs;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Application.Services;
using HealthVerse.Notifications.Domain.Entities;
using MediatR;

namespace HealthVerse.Notifications.Application.Queries;

// ===== Get Notification Preferences Query =====

/// <summary>
/// Kullanıcının bildirim tercihlerini getir.
/// </summary>
public sealed record GetNotificationPreferencesQuery(Guid UserId) : IRequest<NotificationPreferencesResponse>;

public sealed class GetNotificationPreferencesHandler 
    : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesResponse>
{
    private readonly IUserNotificationPreferenceRepository _preferenceRepo;

    public GetNotificationPreferencesHandler(IUserNotificationPreferenceRepository preferenceRepo)
    {
        _preferenceRepo = preferenceRepo;
    }

    public async Task<NotificationPreferencesResponse> Handle(
        GetNotificationPreferencesQuery request,
        CancellationToken ct)
    {
        // Get user's saved preferences
        var savedPreferences = await _preferenceRepo.GetAllByUserAsync(request.UserId, ct);
        var savedDict = savedPreferences.ToDictionary(p => p.Category);

        // Build response with all categories (saved or default)
        var preferences = new List<NotificationPreferenceDto>();

        foreach (var category in Enum.GetValues<NotificationCategory>())
        {
            if (savedDict.TryGetValue(category, out var pref))
            {
                // User has saved preference
                preferences.Add(new NotificationPreferenceDto
                {
                    Category = category.ToString(),
                    DisplayName = GetDisplayName(category),
                    PushEnabled = pref.PushEnabled,
                    QuietHoursStart = pref.QuietHoursStart?.ToString("HH:mm"),
                    QuietHoursEnd = pref.QuietHoursEnd?.ToString("HH:mm")
                });
            }
            else
            {
                // Use default
                preferences.Add(new NotificationPreferenceDto
                {
                    Category = category.ToString(),
                    DisplayName = GetDisplayName(category),
                    PushEnabled = NotificationTypeCategoryMapping.GetDefaultPushEnabled(category),
                    QuietHoursStart = null,
                    QuietHoursEnd = null
                });
            }
        }

        return new NotificationPreferencesResponse
        {
            Preferences = preferences
        };
    }

    private static string GetDisplayName(NotificationCategory category) => category switch
    {
        NotificationCategory.Streak => "Streak Bildirimleri",
        NotificationCategory.Duel => "Düello Bildirimleri",
        NotificationCategory.Task => "Görev Bildirimleri",
        NotificationCategory.Goal => "Hedef Bildirimleri",
        NotificationCategory.PartnerMission => "Partner Görev Bildirimleri",
        NotificationCategory.GlobalMission => "Global Görev Bildirimleri",
        NotificationCategory.League => "Lig Bildirimleri",
        NotificationCategory.Social => "Sosyal Bildirimler",
        NotificationCategory.Milestone => "Başarı Bildirimleri",
        NotificationCategory.System => "Sistem Bildirimleri",
        _ => category.ToString()
    };
}

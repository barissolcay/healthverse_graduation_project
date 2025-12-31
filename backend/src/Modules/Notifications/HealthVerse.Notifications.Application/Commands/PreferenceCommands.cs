using HealthVerse.Notifications.Application.DTOs;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Domain.Entities;
using MediatR;

namespace HealthVerse.Notifications.Application.Commands;

// ===== Update Notification Preferences Command =====

/// <summary>
/// Kullanıcının bildirim tercihlerini güncelle.
/// </summary>
public sealed record UpdateNotificationPreferencesCommand(
    Guid UserId,
    List<UpdatePreferenceItem> Preferences
) : IRequest<UpdatePreferencesResponse>;

public sealed class UpdateNotificationPreferencesHandler 
    : IRequestHandler<UpdateNotificationPreferencesCommand, UpdatePreferencesResponse>
{
    private readonly IUserNotificationPreferenceRepository _preferenceRepo;
    private readonly INotificationsUnitOfWork _unitOfWork;

    public UpdateNotificationPreferencesHandler(
        IUserNotificationPreferenceRepository preferenceRepo,
        INotificationsUnitOfWork unitOfWork)
    {
        _preferenceRepo = preferenceRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdatePreferencesResponse> Handle(
        UpdateNotificationPreferencesCommand request,
        CancellationToken ct)
    {
        if (request.Preferences.Count == 0)
        {
            return new UpdatePreferencesResponse
            {
                Success = true,
                Message = "No preferences to update.",
                UpdatedCount = 0
            };
        }

        int updatedCount = 0;
        var errors = new List<string>();

        foreach (var item in request.Preferences)
        {
            // Parse category (case-insensitive)
            if (!Enum.TryParse<NotificationCategory>(item.Category, ignoreCase: true, out var category))
            {
                errors.Add($"Invalid category: {item.Category}");
                continue;
            }

            // Parse quiet hours
            TimeOnly? quietStart = null;
            TimeOnly? quietEnd = null;

            if (!string.IsNullOrWhiteSpace(item.QuietHoursStart) && 
                !string.IsNullOrWhiteSpace(item.QuietHoursEnd))
            {
                if (!TimeOnly.TryParse(item.QuietHoursStart, out var start))
                {
                    errors.Add($"Invalid QuietHoursStart format for {item.Category}: {item.QuietHoursStart}");
                    continue;
                }
                if (!TimeOnly.TryParse(item.QuietHoursEnd, out var end))
                {
                    errors.Add($"Invalid QuietHoursEnd format for {item.Category}: {item.QuietHoursEnd}");
                    continue;
                }
                quietStart = start;
                quietEnd = end;
            }

            // Get existing preference or create new
            var existing = await _preferenceRepo.GetAsync(request.UserId, category, ct);

            if (existing != null)
            {
                // Update existing
                existing.SetPushEnabled(item.PushEnabled);
                if (quietStart.HasValue && quietEnd.HasValue)
                {
                    existing.SetQuietHours(quietStart, quietEnd);
                }
                else
                {
                    existing.ClearQuietHours();
                }
                await _preferenceRepo.UpdateAsync(existing, ct);
            }
            else
            {
                // Create new
                var newPref = UserNotificationPreference.Create(
                    request.UserId,
                    category,
                    item.PushEnabled,
                    quietStart,
                    quietEnd);
                await _preferenceRepo.AddAsync(newPref, ct);
            }

            updatedCount++;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        if (errors.Count > 0)
        {
            return new UpdatePreferencesResponse
            {
                Success = updatedCount > 0,
                Message = $"Partially updated. Errors: {string.Join("; ", errors)}",
                UpdatedCount = updatedCount
            };
        }

        return new UpdatePreferencesResponse
        {
            Success = true,
            Message = $"Successfully updated {updatedCount} preference(s).",
            UpdatedCount = updatedCount
        };
    }
}

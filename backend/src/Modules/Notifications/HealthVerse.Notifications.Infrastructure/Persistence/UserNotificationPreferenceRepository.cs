using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Notifications.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IUserNotificationPreferenceRepository.
/// </summary>
public sealed class UserNotificationPreferenceRepository : IUserNotificationPreferenceRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public UserNotificationPreferenceRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserNotificationPreference?> GetAsync(
        Guid userId,
        NotificationCategory category,
        CancellationToken ct = default)
    {
        return await _dbContext.UserNotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Category == category, ct);
    }

    public async Task<List<UserNotificationPreference>> GetAllByUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await _dbContext.UserNotificationPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, UserNotificationPreference>> GetByUsersAndCategoryAsync(
        IEnumerable<Guid> userIds,
        NotificationCategory category,
        CancellationToken ct = default)
    {
        var userIdList = userIds.ToList();
        
        var preferences = await _dbContext.UserNotificationPreferences
            .Where(p => userIdList.Contains(p.UserId) && p.Category == category)
            .ToListAsync(ct);

        return preferences.ToDictionary(p => p.UserId);
    }

    public async Task AddAsync(
        UserNotificationPreference preference,
        CancellationToken ct = default)
    {
        await _dbContext.UserNotificationPreferences.AddAsync(preference, ct);
    }

    public Task UpdateAsync(
        UserNotificationPreference preference,
        CancellationToken ct = default)
    {
        _dbContext.UserNotificationPreferences.Update(preference);
        return Task.CompletedTask;
    }

    public async Task UpsertAsync(
        UserNotificationPreference preference,
        CancellationToken ct = default)
    {
        var existing = await GetAsync(preference.UserId, preference.Category, ct);
        
        if (existing == null)
        {
            await AddAsync(preference, ct);
        }
        else
        {
            existing.SetPushEnabled(preference.PushEnabled);
            existing.SetQuietHours(preference.QuietHoursStart, preference.QuietHoursEnd);
        }
    }
}

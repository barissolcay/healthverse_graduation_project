using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Notifications.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of INotificationRepository.
/// </summary>
public sealed class NotificationRepository : INotificationRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public NotificationRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications.FindAsync(new object[] { notificationId }, ct);
    }

    public async Task<List<Notification>> GetRecentByUserAsync(Guid userId, int limit, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await _dbContext.Notifications.AddAsync(notification, ct);
    }

    public async Task<List<Notification>> GetPaginatedByUserAsync(Guid userId, int page, int pageSize, bool unreadOnly, CancellationToken ct = default)
    {
        var query = _dbContext.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1) // Fetch one more to check nextPage
            .ToListAsync(ct);
    }

    public async Task<List<Notification>> GetUnreadByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);
    }

    public async Task<List<Notification>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => ids.Contains(n.Id))
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(Guid userId, bool unreadOnly, CancellationToken ct = default)
    {
        var query = _dbContext.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }
        return await query.CountAsync(ct);
    }
}

using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Notifications.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of INotificationDeliveryRepository.
/// </summary>
public sealed class NotificationDeliveryRepository : INotificationDeliveryRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public NotificationDeliveryRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<NotificationDelivery>> GetReadyToSendAsync(DateTimeOffset now, int take, CancellationToken ct = default)
    {
        return await _dbContext.NotificationDeliveries
            .Where(d => d.Status == DeliveryStatus.Pending && d.ScheduledAt <= now)
            .OrderBy(d => d.ScheduledAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<NotificationDelivery?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.NotificationDeliveries.FindAsync(new object[] { id }, ct);
    }

    public async Task AddAsync(NotificationDelivery delivery, CancellationToken ct = default)
    {
        await _dbContext.NotificationDeliveries.AddAsync(delivery, ct);
    }

    public Task UpdateAsync(NotificationDelivery delivery, CancellationToken ct = default)
    {
        _dbContext.NotificationDeliveries.Update(delivery);
        return Task.CompletedTask;
    }

    public async Task<List<NotificationDelivery>> GetPendingByNotificationAsync(Guid notificationId, CancellationToken ct = default)
    {
        return await _dbContext.NotificationDeliveries
            .Where(d => d.NotificationId == notificationId && d.Status == DeliveryStatus.Pending)
            .ToListAsync(ct);
    }
}

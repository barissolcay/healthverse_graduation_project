using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Notifications.Application.Ports;

namespace HealthVerse.Notifications.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of INotificationsUnitOfWork.
/// </summary>
public sealed class NotificationsUnitOfWork : INotificationsUnitOfWork
{
    private readonly HealthVerseDbContext _dbContext;

    public NotificationsUnitOfWork(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}

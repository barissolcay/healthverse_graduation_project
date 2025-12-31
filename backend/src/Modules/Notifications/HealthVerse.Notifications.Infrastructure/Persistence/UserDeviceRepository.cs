using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Notifications.Application.Ports;
using HealthVerse.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Notifications.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IUserDeviceRepository.
/// </summary>
public sealed class UserDeviceRepository : IUserDeviceRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public UserDeviceRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDevice?> GetByTokenAsync(string pushToken, CancellationToken ct = default)
    {
        return await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.PushToken == pushToken, ct);
    }

    public async Task<List<UserDevice>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserDevices
            .Where(d => d.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<List<UserDevice>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserDevices
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync(ct);
    }

    public async Task AddAsync(UserDevice device, CancellationToken ct = default)
    {
        await _dbContext.UserDevices.AddAsync(device, ct);
    }

    public Task RemoveAsync(UserDevice device, CancellationToken ct = default)
    {
        _dbContext.UserDevices.Remove(device);
        return Task.CompletedTask;
    }

    public async Task DisableAsync(Guid deviceId, CancellationToken ct = default)
    {
        var device = await _dbContext.UserDevices.FindAsync(new object[] { deviceId }, ct);
        if (device != null)
        {
            device.Disable();
        }
    }
}

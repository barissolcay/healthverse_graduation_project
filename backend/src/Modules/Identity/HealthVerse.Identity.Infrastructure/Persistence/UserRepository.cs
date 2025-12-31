using AppPorts = HealthVerse.Identity.Application.Ports;
using DomainPorts = HealthVerse.Identity.Domain.Ports;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IUserRepository.
/// Implements both Application and Domain ports for cross-module access.
/// </summary>
public sealed class UserRepository : AppPorts.IUserRepository, DomainPorts.IUserRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public UserRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Users.FindAsync(new object[] { userId }, ct);
    }

    public async Task<Dictionary<Guid, User>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var idList = userIds.ToList();
        if (idList.Count == 0)
            return new Dictionary<Guid, User>();

        var users = await _dbContext.Users
            .Where(u => idList.Contains(u.Id))
            .ToListAsync(ct);

        return users.ToDictionary(u => u.Id);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        // Value object comparison needs to match how it's mapped in EF Core.
        // Assuming implicit conversion or properties are mapped.
        // For accurate comparison, we might need to check how Username value object is mapped.
        // Usually it's mapped via OwnsOne or conversion.
        // If it's a simple property mapped to column, we can use LINQ.
        // But since Username is a value object, let's assume it's mapped as owned entity or converted.
        // Checking HealthVerseDbContext mapping might be needed, but sticking to basics for now.
        // If it fails, we'll fix it.
        
        // Assuming Username is mapped to a column 'Username'
        // If mapped as owned type, it might be u.Username.Value
        
        // Safe bet: Fetch matching by string if mapped
        return await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Username.Value == username, ct);
    }

    public async Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Username.Value == username, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _dbContext.Users.AddAsync(user, ct);
    }
    public async Task<List<User>> GetTopByPointsAsync(int limit, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .OrderByDescending(u => u.TotalPoints)
            .Take(limit)
            .ToListAsync(ct);
    }

    // --- Domain.Ports.IUserRepository Implementation ---

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email.Value == email, ct);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await IsUsernameTakenAsync(username, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Email.Value == email, ct);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }

    public void Remove(User user)
    {
        _dbContext.Users.Remove(user);
    }
}

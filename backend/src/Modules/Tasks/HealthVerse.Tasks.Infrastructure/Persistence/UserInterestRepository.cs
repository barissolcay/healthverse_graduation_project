using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Tasks.Application.Ports;
using HealthVerse.Tasks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Tasks.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IUserInterestRepository.
/// </summary>
public sealed class UserInterestRepository : IUserInterestRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public UserInterestRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<string>> GetActivityTypesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserInterests
            .Where(i => i.UserId == userId)
            .Select(i => i.ActivityType)
            .ToListAsync(ct);
    }

    public async Task ReplaceAsync(Guid userId, IEnumerable<string> activityTypes, CancellationToken ct = default)
    {
        // Remove existing interests
        var existing = await _dbContext.UserInterests
            .Where(i => i.UserId == userId)
            .ToListAsync(ct);

        _dbContext.UserInterests.RemoveRange(existing);

        // Add new interests
        var newInterests = activityTypes
            .Select(at => UserInterest.Create(userId, at))
            .ToList();

        await _dbContext.UserInterests.AddRangeAsync(newInterests, ct);
    }
}

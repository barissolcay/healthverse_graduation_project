using HealthVerse.Competition.Application.Ports;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Competition.Infrastructure.Persistence;

public sealed class CompetitionUserRepository : ICompetitionUserRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public CompetitionUserRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Users.FindAsync(new object[] { userId }, ct);
    }

    public async Task<Dictionary<Guid, User>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        return await _dbContext.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);
    }
}

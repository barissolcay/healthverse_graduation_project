using HealthVerse.Identity.Application.Ports;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IAuthIdentityRepository.
/// </summary>
public sealed class AuthIdentityRepository : IAuthIdentityRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public AuthIdentityRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthIdentity?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken ct = default)
    {
        return await _dbContext.AuthIdentities
            .FirstOrDefaultAsync(a => a.FirebaseUid == firebaseUid, ct);
    }

    public async Task<AuthIdentity?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.AuthIdentities
            .FirstOrDefaultAsync(a => a.ProviderEmail == email, ct);
    }

    public async Task<List<AuthIdentity>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.AuthIdentities
            .Where(a => a.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(AuthIdentity authIdentity, CancellationToken ct = default)
    {
        await _dbContext.AuthIdentities.AddAsync(authIdentity, ct);
    }
}

using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVerse.Gamification.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IMilestoneRepository.
/// </summary>
public sealed class MilestoneRepository : IMilestoneRepository
{
    private readonly HealthVerseDbContext _dbContext;

    public MilestoneRepository(HealthVerseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MilestoneReward>> GetAllDefinitionsAsync(CancellationToken ct = default)
    {
        return await _dbContext.MilestoneRewards.ToListAsync(ct);
    }

    public async Task<List<MilestoneReward>> GetActiveDefinitionsAsync(CancellationToken ct = default)
    {
        return await _dbContext.MilestoneRewards
            .Where(m => m.IsActive)
            .ToListAsync(ct);
    }

    public async Task<MilestoneReward?> GetDefinitionByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.MilestoneRewards.FindAsync(new object[] { id }, ct);
    }

    public async Task<UserMilestone?> GetUserProgressAsync(Guid userId, Guid milestoneId, CancellationToken ct = default)
    {
        return await _dbContext.UserMilestones
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MilestoneRewardId == milestoneId, ct);
    }

    public async Task<List<UserMilestone>> GetUserMilestonesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserMilestones
            .Where(um => um.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task AddUserMilestoneAsync(UserMilestone userMilestone, CancellationToken ct = default)
    {
        await _dbContext.UserMilestones.AddAsync(userMilestone, ct);
    }
}

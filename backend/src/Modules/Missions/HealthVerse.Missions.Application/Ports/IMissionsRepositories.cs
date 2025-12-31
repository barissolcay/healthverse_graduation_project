using HealthVerse.Missions.Domain.Entities;

namespace HealthVerse.Missions.Application.Ports;

/// <summary>
/// Partner mission repository port.
/// </summary>
public interface IPartnerMissionRepository
{
    /// <summary>
    /// Get a partner mission by ID.
    /// </summary>
    Task<WeeklyPartnerMission?> GetByIdAsync(Guid missionId, CancellationToken ct = default);

    /// <summary>
    /// Get active partner mission for a user in a specific week.
    /// </summary>
    Task<WeeklyPartnerMission?> GetActiveByUserAsync(string weekId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get partner mission history for a user.
    /// </summary>
    Task<List<WeeklyPartnerMission>> GetHistoryByUserAsync(Guid userId, int limit, CancellationToken ct = default);

    /// <summary>
    /// Add a new partner mission.
    /// </summary>
    Task AddAsync(WeeklyPartnerMission mission, CancellationToken ct = default);
}

/// <summary>
/// Partner mission slot repository port.
/// </summary>
public interface IPartnerMissionSlotRepository
{
    /// <summary>
    /// Check if a user is busy (already paired) in a specific week.
    /// </summary>
    Task<bool> IsUserBusyAsync(string weekId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Add a new partner mission slot.
    /// </summary>
    Task AddAsync(WeeklyPartnerMissionSlot slot, CancellationToken ct = default);
}

/// <summary>
/// Global mission repository port.
/// </summary>
public interface IGlobalMissionRepository
{
    /// <summary>
    /// Get active global missions.
    /// </summary>
    Task<List<GlobalMission>> GetActiveAsync(DateTimeOffset now, CancellationToken ct = default);

    /// <summary>
    /// Get finished global missions (history).
    /// </summary>
    Task<List<GlobalMission>> GetHistoryAsync(DateTimeOffset now, int limit, CancellationToken ct = default);

    /// <summary>
    /// Get a global mission by ID.
    /// </summary>
    Task<GlobalMission?> GetByIdAsync(Guid missionId, CancellationToken ct = default);

    /// <summary>
    /// Update the mission cache (CurrentValue/Status).
    /// </summary>
    Task UpdateCacheAsync(GlobalMission mission, CancellationToken ct = default);
}

/// <summary>
/// Global mission participant repository port.
/// </summary>
public interface IGlobalMissionParticipantRepository
{
    /// <summary>
    /// Get a participant by mission and user.
    /// </summary>
    Task<GlobalMissionParticipant?> GetAsync(Guid missionId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Add a new participant.
    /// </summary>
    Task AddAsync(GlobalMissionParticipant participant, CancellationToken ct = default);

    /// <summary>
    /// Get participants for a user across multiple missions.
    /// </summary>
    Task<Dictionary<Guid, GlobalMissionParticipant>> GetByUserAsync(Guid userId, IEnumerable<Guid> missionIds, CancellationToken ct = default);

    /// <summary>
    /// Get top contributors for a mission.
    /// </summary>
    Task<List<GlobalMissionParticipant>> GetTopContributorsAsync(Guid missionId, int take, CancellationToken ct = default);

    /// <summary>
    /// Count total participants in a mission.
    /// </summary>
    Task<int> CountAsync(Guid missionId, CancellationToken ct = default);
}

/// <summary>
/// Global mission contribution repository port.
/// </summary>
public interface IGlobalMissionContributionRepository
{
    /// <summary>
    /// Check if a contribution with the given idempotency key exists.
    /// </summary>
    Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Add a new contribution.
    /// </summary>
    Task AddAsync(GlobalMissionContribution contribution, CancellationToken ct = default);
}

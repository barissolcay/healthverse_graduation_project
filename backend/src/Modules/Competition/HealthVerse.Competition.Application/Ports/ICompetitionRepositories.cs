using HealthVerse.Competition.Domain.Entities;
using HealthVerse.SharedKernel.ValueObjects;

namespace HealthVerse.Competition.Application.Ports;

/// <summary>
/// League room repository port.
/// </summary>
public interface ILeagueRoomRepository
{
    Task<List<LeagueRoom>> GetUnprocessedByWeekAsync(WeekId weekId, CancellationToken ct = default);
    Task<LeagueRoom?> GetByIdAsync(Guid roomId, CancellationToken ct = default);
    Task<LeagueRoom?> FindAvailableForTierAsync(WeekId weekId, string tier, int maxRoomSize, CancellationToken ct = default);
    Task AddAsync(LeagueRoom room, CancellationToken ct = default);
    Task IncrementUserCountAsync(Guid roomId, CancellationToken ct = default);
}

/// <summary>
/// League member repository port.
/// </summary>
public interface ILeagueMemberRepository
{
    Task<LeagueMember?> GetMembershipByUserAndWeekAsync(Guid userId, WeekId weekId, CancellationToken ct = default);
    Task<List<LeagueMember>> GetMembersByRoomOrderedAsync(Guid roomId, CancellationToken ct = default);
    Task<int> CountByRoomAsync(Guid roomId, CancellationToken ct = default);
    Task<int> GetRankForUserAsync(Guid roomId, int userPoints, CancellationToken ct = default);
    Task AddAsync(LeagueMember member, CancellationToken ct = default);
}

/// <summary>
/// League config repository port.
/// </summary>
public interface ILeagueConfigRepository
{
    Task<LeagueConfig?> GetByTierNameAsync(string tierName, CancellationToken ct = default);
    Task<List<LeagueConfig>> GetAllOrderedAsync(CancellationToken ct = default);
    Task<LeagueConfig?> GetNextByOrderAsync(int tierOrder, CancellationToken ct = default);
    Task<LeagueConfig?> GetPrevByOrderAsync(int tierOrder, CancellationToken ct = default);
}

/// <summary>
/// User points history repository port.
/// </summary>
public interface IUserPointsHistoryRepository
{
    Task AddWeeklyHistoryAsync(UserPointsHistory history, CancellationToken ct = default);
    Task<List<UserPointsHistory>> GetWeeklyHistoryAsync(Guid userId, int limit, CancellationToken ct = default);
}

using HealthVerse.Gamification.Application.DTOs;
using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Gamification.Domain.Services;
using HealthVerse.Identity.Domain.Ports;
using HealthVerse.SharedKernel.Abstractions;
using MediatR;

namespace HealthVerse.Gamification.Application.Queries;

// --- Queries ---

public sealed record GetStreakDetailQuery(Guid UserId) : IRequest<StreakDetailResponse>;
public sealed record GetPointsHistoryQuery(Guid UserId, int Days = 30) : IRequest<PointsHistoryResponse>;
public sealed record GetUserStatsQuery(Guid UserId) : IRequest<UserStatsResponse>;

// --- Handlers ---

public sealed class UserGamificationQueryHandlers :
    IRequestHandler<GetStreakDetailQuery, StreakDetailResponse>,
    IRequestHandler<GetPointsHistoryQuery, PointsHistoryResponse>,
    IRequestHandler<GetUserStatsQuery, UserStatsResponse>
{
    private readonly IUserDailyStatsRepository _dailyStatsRepo;
    private readonly IUserRepository _userRepo;
    private readonly IClock _clock;

    public UserGamificationQueryHandlers(
        IUserDailyStatsRepository dailyStatsRepo,
        IUserRepository userRepo,
        IClock clock)
    {
        _dailyStatsRepo = dailyStatsRepo;
        _userRepo = userRepo;
        _clock = clock;
    }

    public async Task<UserStatsResponse> Handle(GetUserStatsQuery request, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user == null)
             throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        return new UserStatsResponse
        {
            UserId = user.Id,
            Username = user.Username.Value,
            AvatarId = user.AvatarId,
            TotalPoints = user.TotalPoints,
            StreakCount = user.StreakCount,
            LongestStreakCount = user.LongestStreakCount,
            FreezeInventory = user.FreezeInventory,
            TotalTasksCompleted = user.TotalTasksCompleted,
            TotalDuelsWon = user.TotalDuelsWon,
            TotalGlobalMissions = user.TotalGlobalMissions,
            FollowingCount = user.FollowingCount,
            FollowersCount = user.FollowersCount,
            CurrentTier = user.CurrentTier,
            SelectedTitleId = user.SelectedTitleId
        };
    }

    public async Task<StreakDetailResponse> Handle(GetStreakDetailQuery request, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user == null)
             // In real app, maybe catch or return null. Controller will handle mapping if implementation throws "NotFoundException" or we return Null/Error object.
             // For now, let's throw to follow quick migration, or better returning a response indicating failure?
             // The controller returned NotFound. Application layer usually throws generic "NotFoundException".
             // Given I don't want to add new Exceptions right now, I'll return null or empty? 
             // Let's assume user exists for authenticated requests, or return empty/default.
             // Best practice: Throw NotFoundException.
             throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var today = _clock.TodayTR;
        var todayStats = await _dailyStatsRepo.GetByDateAsync(request.UserId, today, ct);

        var todaySteps = todayStats?.DailySteps ?? 0;

        var progressPercent = Math.Min(100, (int)((todaySteps / (double)StreakService.StreakThreshold) * 100));

        return new StreakDetailResponse
        {
            StreakCount = user.StreakCount,
            LongestStreakCount = user.LongestStreakCount,
            TodaySteps = todaySteps,
            StreakThreshold = StreakService.StreakThreshold,
            TodayProgressPercent = progressPercent,
            FreezeInventory = user.FreezeInventory,
            StreakSecuredToday = todaySteps >= StreakService.StreakThreshold,
            LastStreakDate = user.LastStreakDate
        };
    }

    public async Task<PointsHistoryResponse> Handle(GetPointsHistoryQuery request, CancellationToken ct)
    {
        var days = Math.Min(Math.Max(request.Days, 1), 90);
        var startDate = _clock.TodayTR.AddDays(-days + 1);
        var endDate = _clock.TodayTR;

        var dailyStats = await _dailyStatsRepo.GetByRangeAsync(request.UserId, startDate, endDate, ct);
        
        // Ordering by date is done in repository usually, but safe to re-order here
        var orderedStats = dailyStats.OrderBy(s => s.LogDate).ToList();

        long runningTotal = 0;
        var history = orderedStats.Select(s =>
        {
            runningTotal += s.DailyPoints;
            return new PointHistoryItem
            {
                Date = s.LogDate,
                DailyPoints = s.DailyPoints,
                DailySteps = s.DailySteps,
                RunningTotal = runningTotal
            };
        }).ToList();

        return new PointsHistoryResponse
        {
            UserId = request.UserId,
            History = history,
            TotalDays = history.Count,
            TotalPointsInPeriod = history.Sum(h => h.DailyPoints),
            TotalStepsInPeriod = history.Sum(h => h.DailySteps)
        };
    }
}

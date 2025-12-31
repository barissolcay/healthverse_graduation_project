using HealthVerse.Gamification.Application.DTOs;
using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.Ports;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;

namespace HealthVerse.Gamification.Application.Queries;

// --- Queries ---

/// <summary>
/// Get leaderboard for a specific period (WEEKLY, MONTHLY, ALLTIME).
/// </summary>
public sealed record GetLeaderboardQuery(string PeriodType, Guid? CurrentUserId = null, int Limit = 50) : IRequest<LeaderboardResponse>;

// --- Handlers ---

public sealed class LeaderboardQueryHandlers : IRequestHandler<GetLeaderboardQuery, LeaderboardResponse>
{
    private readonly IUserDailyStatsRepository _dailyStatsRepo;
    private readonly IUserRepository _userRepo;
    private readonly IClock _clock;

    public LeaderboardQueryHandlers(
        IUserDailyStatsRepository dailyStatsRepo,
        IUserRepository userRepo,
        IClock clock)
    {
        _dailyStatsRepo = dailyStatsRepo;
        _userRepo = userRepo;
        _clock = clock;
    }

    public async Task<LeaderboardResponse> Handle(GetLeaderboardQuery request, CancellationToken ct)
    {
        var limit = Math.Min(Math.Max(request.Limit, 10), 100);
        var periodType = request.PeriodType.ToUpperInvariant();
        var currentUserId = request.CurrentUserId ?? Guid.Empty;
        
        List<(Guid UserId, long TotalPoints)> rankedUsers = new();
        string periodId = "";

        if (periodType == "WEEKLY")
        {
            var weekStart = GetWeekStartDate(_clock.TodayTR);
            var weekEnd = weekStart.AddDays(6);
            rankedUsers = await _dailyStatsRepo.GetTopUsersByDateRangeAsync(weekStart, weekEnd, limit, ct);
            periodId = WeekId.FromDate(_clock.TodayTR).Value;
        }
        else if (periodType == "MONTHLY")
        {
            var today = _clock.TodayTR;
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            rankedUsers = await _dailyStatsRepo.GetTopUsersByDateRangeAsync(monthStart, monthEnd, limit, ct);
            periodId = today.ToString("yyyy-MM");
        }
        if (periodType == "ALLTIME")
        {
            var users = await _userRepo.GetTopByPointsAsync(limit, ct);
            var allTimeLeaderboard = MapToDto(users, currentUserId);
            return new LeaderboardResponse
            {
                PeriodType = periodType,
                PeriodId = "ALL",
                Users = allTimeLeaderboard,
                CurrentUserRank = allTimeLeaderboard.FirstOrDefault(u => u.IsCurrentUser),
                TotalUsers = users.Count
            };
        }
        else
        {
            // WEEKLY or MONTHLY
            if (periodType != "WEEKLY" && periodType != "MONTHLY")
                 throw new ArgumentException("Invalid period type. Use WEEKLY, MONTHLY, or ALLTIME.");
        }

        // Fetch User Details for Weekly/Monthly
        var userIds = rankedUsers.Select(x => x.UserId).ToList();
        var userDict = await _userRepo.GetByIdsAsync(userIds, ct);

        var leaderboard = new List<LeaderboardUserDto>();
        for (int i = 0; i < rankedUsers.Count; i++)
        {
            var (uid, points) = rankedUsers[i];
            var user = userDict.ContainsKey(uid) ? userDict[uid] : null;

            leaderboard.Add(new LeaderboardUserDto
            {
                Rank = i + 1,
                UserId = uid,
                Username = user?.Username.Value ?? "Unknown",
                AvatarId = user?.AvatarId ?? 1,
                CurrentTier = user?.CurrentTier ?? "ISINMA",
                Points = points,
                IsCurrentUser = uid == currentUserId
            });
        }

        return new LeaderboardResponse
        {
            PeriodType = periodType,
            PeriodId = periodId,
            Users = leaderboard,
            CurrentUserRank = leaderboard.FirstOrDefault(u => u.IsCurrentUser),
            TotalUsers = leaderboard.Count
        };
    }

    private List<LeaderboardUserDto> MapToDto(IEnumerable<User> users, Guid currentUserId)
    {
        return users.Select((u, i) => new LeaderboardUserDto
        {
            Rank = i + 1,
            UserId = u.Id,
            Username = u.Username.Value,
            AvatarId = u.AvatarId,
            CurrentTier = u.CurrentTier,
            Points = u.TotalPoints,
            IsCurrentUser = u.Id == currentUserId
        }).ToList();
    }


    private static DateOnly GetWeekStartDate(DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return date.AddDays(-daysToMonday);
    }
}

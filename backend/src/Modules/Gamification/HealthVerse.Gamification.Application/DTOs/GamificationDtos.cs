namespace HealthVerse.Gamification.Application.DTOs;

/// <summary>
/// Kullanıcı istatistikleri DTO'su.
/// </summary>
public sealed class UserStatsResponse
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public int AvatarId { get; init; }
    
    // ===== Gamification =====
    public long TotalPoints { get; init; }
    public int StreakCount { get; init; }
    public int LongestStreakCount { get; init; }
    public int FreezeInventory { get; init; }
    
    // ===== Statistics =====
    public int TotalTasksCompleted { get; init; }
    public int TotalDuelsWon { get; init; }
    public int TotalGlobalMissions { get; init; }
    
    // ===== Social =====
    public int FollowingCount { get; init; }
    public int FollowersCount { get; init; }
    
    // ===== League =====
    public string CurrentTier { get; init; } = null!;
    public string? SelectedTitleId { get; init; }
}

/// <summary>
/// Puan geçmişi için tek kayıt.
/// </summary>
public sealed class PointHistoryItem
{
    public DateOnly Date { get; init; }
    public int DailyPoints { get; init; }
    public int DailySteps { get; init; }
    public long RunningTotal { get; init; }
}

/// <summary>
/// Puan geçmişi sonucu.
/// </summary>
public sealed class PointsHistoryResponse
{
    public Guid UserId { get; init; }
    public List<PointHistoryItem> History { get; init; } = new();
    public int TotalDays { get; init; }
    public long TotalPointsInPeriod { get; init; }
    public int TotalStepsInPeriod { get; init; }
}

/// <summary>
/// Sıralama listesi için kullanıcı bilgisi.
/// </summary>
public sealed class LeaderboardUserDto
{
    public int Rank { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public int AvatarId { get; init; }
    public string CurrentTier { get; init; } = null!;
    public long Points { get; init; }
    public bool IsCurrentUser { get; init; }
}

/// <summary>
/// Sıralama listesi sonucu.
/// </summary>
public sealed class LeaderboardResponse
{
    public string PeriodType { get; init; } = null!; // WEEKLY, MONTHLY, ALLTIME
    public string PeriodId { get; init; } = null!;   // 2025-W01, 2025-01, ALL
    public List<LeaderboardUserDto> Users { get; init; } = new();
    public LeaderboardUserDto? CurrentUserRank { get; init; }
    public int TotalUsers { get; init; }
}

namespace HealthVerse.Missions.Application.DTOs;

// ===== Global Mission DTOs =====

/// <summary>
/// Global görev detay bilgisi.
/// </summary>
public sealed class GlobalMissionDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? ActivityType { get; init; }
    public string TargetMetric { get; init; } = null!;
    public long TargetValue { get; init; }
    public long CurrentValue { get; init; }
    public int ProgressPercent { get; init; }
    public string Status { get; init; } = null!;
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public int HoursRemaining { get; init; }
    
    // Kullanıcı bilgileri
    public bool IsParticipant { get; init; }
    public long MyContribution { get; init; }
    public bool IsRewardClaimed { get; init; }
    
    // Top katkıcılar
    public List<TopContributorDto> TopContributors { get; init; } = new();
    public int TotalParticipants { get; init; }
}

/// <summary>
/// Top katkıcı bilgisi.
/// </summary>
public sealed class TopContributorDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public int AvatarId { get; init; }
    public long ContributionValue { get; init; }
    public int Rank { get; init; }
}

/// <summary>
/// Aktif global görevler response.
/// </summary>
public sealed class ActiveGlobalMissionsResponse
{
    public List<GlobalMissionDetailDto> Missions { get; init; } = new();
    public int TotalActive { get; init; }
}

/// <summary>
/// Global göreve katılım response.
/// </summary>
public sealed class JoinMissionResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}

// ===== Partner Mission DTOs =====

/// <summary>
/// Partner görevi detay bilgisi.
/// </summary>
public sealed class PartnerMissionDetailDto
{
    public Guid Id { get; init; }
    public string WeekId { get; init; } = null!;
    
    public Guid InitiatorId { get; init; }
    public string InitiatorName { get; init; } = null!;
    public int InitiatorAvatarId { get; init; }
    public int InitiatorProgress { get; init; }
    
    public Guid PartnerId { get; init; }
    public string PartnerName { get; init; } = null!;
    public int PartnerAvatarId { get; init; }
    public int PartnerProgress { get; init; }
    
    public string? ActivityType { get; init; }
    public string TargetMetric { get; init; } = null!;
    public int TargetValue { get; init; }
    public int TotalProgress { get; init; }
    public int ProgressPercent { get; init; }
    
    public string Status { get; init; } = null!;
    public bool IsCompleted { get; init; }
    public bool IsInitiator { get; init; }
    public bool CanPoke { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Boşta arkadaş listesi response.
/// </summary>
public sealed class AvailableFriendsResponse
{
    public List<AvailableFriendDto> Friends { get; init; } = new();
    public int TotalAvailable { get; init; }
}

/// <summary>
/// Boşta arkadaş bilgisi.
/// </summary>
public sealed class AvailableFriendDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public int AvatarId { get; init; }
}

/// <summary>
/// Partner eşleşme response.
/// </summary>
public sealed class PairPartnerResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public Guid? MissionId { get; init; }
}

/// <summary>
/// Partner görevi action response.
/// </summary>
public sealed class PartnerMissionActionResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}

/// <summary>
/// Partner görevi geçmişi response.
/// </summary>
public sealed class PartnerMissionHistoryResponse
{
    public List<PartnerMissionDetailDto> Missions { get; init; } = new();
    public int TotalCompleted { get; init; }
}

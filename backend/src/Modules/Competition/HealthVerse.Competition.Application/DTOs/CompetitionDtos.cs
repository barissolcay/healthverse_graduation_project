namespace HealthVerse.Competition.Application.DTOs;

/// <summary>
/// Kullanıcının mevcut lig odası bilgisi.
/// </summary>
public sealed class MyRoomResponse
{
    public Guid RoomId { get; init; }
    public string WeekId { get; init; } = null!;
    public string Tier { get; init; } = null!;
    public int TierOrder { get; init; }
    public int RankInRoom { get; init; }
    public int PointsInRoom { get; init; }
    public int TotalMembers { get; init; }
    public DateTimeOffset StartsAt { get; init; }
    public DateTimeOffset EndsAt { get; init; }
    public bool IsProcessed { get; init; }
    
    /// <summary>
    /// Bu haftanın kalan süresi (saat cinsinden).
    /// </summary>
    public int HoursRemaining { get; init; }
}

/// <summary>
/// Oda sıralaması için kullanıcı bilgisi.
/// </summary>
public sealed class RoomMemberDto
{
    public int Rank { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public int AvatarId { get; init; }
    public int PointsInRoom { get; init; }
    
    /// <summary>
    /// Promosyon bölgesinde mi? (Yeşil)
    /// </summary>
    public bool InPromotionZone { get; init; }
    
    /// <summary>
    /// Dereceli bölgede mi? (Kırmızı)
    /// </summary>
    public bool InDemotionZone { get; init; }
    
    public bool IsCurrentUser { get; init; }
}

/// <summary>
/// Oda sıralaması response.
/// </summary>
public sealed class RoomLeaderboardResponse
{
    public Guid RoomId { get; init; }
    public string WeekId { get; init; } = null!;
    public string Tier { get; init; } = null!;
    public int TotalMembers { get; init; }
    public int PromotionCutoff { get; init; }
    public int DemotionCutoff { get; init; }
    public List<RoomMemberDto> Members { get; init; } = new();
}

/// <summary>
/// Tier bilgisi.
/// </summary>
public sealed class TierInfoDto
{
    public string TierName { get; init; } = null!;
    public int TierOrder { get; init; }
    public int PromotePercentage { get; init; }
    public int DemotePercentage { get; init; }
    public int MinRoomSize { get; init; }
    public int MaxRoomSize { get; init; }
    public bool IsLowestTier { get; init; }
    public bool IsHighestTier { get; init; }
}

/// <summary>
/// Tier listesi response.
/// </summary>
public sealed class TiersResponse
{
    public List<TierInfoDto> Tiers { get; init; } = new();
    public string UserCurrentTier { get; init; } = null!;
}

/// <summary>
/// Geçmiş hafta özeti.
/// </summary>
public sealed class LeagueHistoryItem
{
    public string WeekId { get; init; } = null!;
    public string Tier { get; init; } = null!;
    public int Points { get; init; }
    public int? Rank { get; init; }
    public string Result { get; init; } = null!; // PROMOTED, DEMOTED, STAYED
}

/// <summary>
/// Geçmiş hafta listesi response.
/// </summary>
public sealed class LeagueHistoryResponse
{
    public List<LeagueHistoryItem> History { get; init; } = new();
    public int TotalWeeks { get; init; }
}

/// <summary>
/// Lige katılım response.
/// </summary>
public sealed class JoinLeagueResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public Guid? RoomId { get; init; }
    public string? WeekId { get; init; }
    public string? Tier { get; init; }
}

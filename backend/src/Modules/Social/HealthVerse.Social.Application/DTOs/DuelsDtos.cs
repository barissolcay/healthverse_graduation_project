namespace HealthVerse.Social.Application.DTOs;

// ===== Duel DTOs =====

/// <summary>
/// Düello oluşturma request.
/// </summary>
public sealed class CreateDuelRequest
{
    public Guid OpponentId { get; init; }
    public string ActivityType { get; init; } = null!;
    public string TargetMetric { get; init; } = null!;
    public int TargetValue { get; init; }
    public int DurationDays { get; init; }
}

/// <summary>
/// Düello oluşturma response.
/// </summary>
public sealed class CreateDuelResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public Guid? DuelId { get; init; }
}

/// <summary>
/// Düello detay bilgisi.
/// </summary>
public sealed class DuelDetailDto
{
    public Guid Id { get; init; }
    
    // Taraflar
    public Guid ChallengerId { get; init; }
    public string ChallengerName { get; init; } = null!;
    public int ChallengerAvatarId { get; init; }
    public Guid OpponentId { get; init; }
    public string OpponentName { get; init; } = null!;
    public int OpponentAvatarId { get; init; }
    
    // Düello bilgileri
    public string ActivityType { get; init; } = null!;
    public string TargetMetric { get; init; } = null!;
    public int TargetValue { get; init; }
    public int DurationDays { get; init; }
    
    // Durum
    public string Status { get; init; } = null!;
    public int ChallengerScore { get; init; }
    public int OpponentScore { get; init; }
    public int ChallengerProgressPercent { get; init; }
    public int OpponentProgressPercent { get; init; }
    public string? Result { get; init; }
    
    // Tarihler
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public int? HoursRemaining { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    
    // Kullanıcı perspektifi
    public bool IsChallenger { get; init; }
    public bool CanPoke { get; init; }
}

/// <summary>
/// Bekleyen davetler response.
/// </summary>
public sealed class PendingDuelsResponse
{
    public List<DuelDetailDto> Incoming { get; init; } = new();
    public List<DuelDetailDto> Outgoing { get; init; } = new();
    public int TotalPending { get; init; }
}

/// <summary>
/// Aktif düellolar response.
/// </summary>
public sealed class ActiveDuelsResponse
{
    public List<DuelDetailDto> Duels { get; init; } = new();
    public int TotalActive { get; init; }
}

/// <summary>
/// Düello geçmişi response.
/// </summary>
public sealed class DuelHistoryResponse
{
    public List<DuelDetailDto> Duels { get; init; } = new();
    public int TotalCompleted { get; init; }
}

/// <summary>
/// Düello işlem response (accept/reject/poke).
/// </summary>
public sealed class DuelActionResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}

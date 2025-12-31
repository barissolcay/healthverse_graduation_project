using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Social.Domain.Entities;

/// <summary>
/// Düello entity - 1v1 rekabet sistemi.
/// 
/// Akış:
/// WAITING -> ACTIVE (kabul edildi) -> FINISHED (süre doldu veya hedef tamamlandı)
/// WAITING -> REJECTED (reddedildi)
/// WAITING -> EXPIRED (24 saat içinde yanıt yok)
/// </summary>
public sealed class Duel : Entity
{
    public Guid ChallengerId { get; private init; }
    public Guid OpponentId { get; private init; }
    
    /// <summary>
    /// Aktivite tipi: RUNNING, WALKING, CYCLING, SWIMMING vb.
    /// </summary>
    public string ActivityType { get; private init; } = null!;
    
    /// <summary>
    /// Hedef metriği: STEPS, DURATION, CALORIES, DISTANCE vb.
    /// </summary>
    public string TargetMetric { get; private init; } = null!;
    
    public int TargetValue { get; private init; }
    
    /// <summary>
    /// Düello süresi (1-7 gün).
    /// </summary>
    public int DurationDays { get; private init; }
    
    /// <summary>
    /// Düello durumu.
    /// </summary>
    public string Status { get; private set; } = null!;
    
    public int ChallengerScore { get; private set; }
    public int OpponentScore { get; private set; }
    
    /// <summary>
    /// Sonuç: Sadece FINISHED durumunda set edilir.
    /// </summary>
    public string? Result { get; private set; }
    
    public DateTimeOffset? StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    
    public DateTimeOffset? ChallengerLastPokeAt { get; private set; }
    public DateTimeOffset? OpponentLastPokeAt { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Duel() { }

    /// <summary>
    /// Yeni düello daveti oluşturur.
    /// </summary>
    public static Duel Create(
        Guid challengerId,
        Guid opponentId,
        string activityType,
        string targetMetric,
        int targetValue,
        int durationDays)
    {
        if (challengerId == Guid.Empty)
            throw new DomainException("Duel.InvalidChallenger", "ChallengerId cannot be empty.");

        if (opponentId == Guid.Empty)
            throw new DomainException("Duel.InvalidOpponent", "OpponentId cannot be empty.");

        if (challengerId == opponentId)
            throw new DomainException("Duel.SelfDuel", "Cannot duel yourself.");

        if (string.IsNullOrWhiteSpace(activityType))
            throw new DomainException("Duel.InvalidActivity", "ActivityType cannot be empty.");

        if (string.IsNullOrWhiteSpace(targetMetric))
            throw new DomainException("Duel.InvalidMetric", "TargetMetric cannot be empty.");

        if (targetValue <= 0)
            throw new DomainException("Duel.InvalidTarget", "TargetValue must be positive.");

        if (durationDays < 1 || durationDays > 7)
            throw new DomainException("Duel.InvalidDuration", "DurationDays must be between 1 and 7.");

        var now = DateTimeOffset.UtcNow;
        return new Duel
        {
            Id = Guid.NewGuid(),
            ChallengerId = challengerId,
            OpponentId = opponentId,
            ActivityType = activityType.ToUpperInvariant(),
            TargetMetric = targetMetric.ToUpperInvariant(),
            TargetValue = targetValue,
            DurationDays = durationDays,
            Status = DuelStatus.WAITING,
            ChallengerScore = 0,
            OpponentScore = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Daveti kabul et ve düelloyu başlat.
    /// </summary>
    public bool Accept(DateTimeOffset now)
    {
        if (Status != DuelStatus.WAITING)
            return false;

        Status = DuelStatus.ACTIVE;
        StartDate = now;
        EndDate = now.AddDays(DurationDays);
        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// Daveti reddet.
    /// </summary>
    public bool Reject(DateTimeOffset now)
    {
        if (Status != DuelStatus.WAITING)
            return false;

        Status = DuelStatus.REJECTED;
        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// 24 saat içinde yanıtlanmazsa expire et.
    /// </summary>
    public bool Expire(DateTimeOffset now)
    {
        if (Status != DuelStatus.WAITING)
            return false;

        // 24 saati geçti mi?
        if ((now - CreatedAt).TotalHours < 24)
            return false;

        Status = DuelStatus.EXPIRED;
        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// Challenger skorunu güncelle.
    /// </summary>
    public void UpdateChallengerScore(int newScore, DateTimeOffset now)
    {
        if (Status != DuelStatus.ACTIVE)
            return;

        ChallengerScore = Math.Max(0, Math.Min(newScore, TargetValue));
        UpdatedAt = now;
    }

    /// <summary>
    /// Opponent skorunu güncelle.
    /// </summary>
    public void UpdateOpponentScore(int newScore, DateTimeOffset now)
    {
        if (Status != DuelStatus.ACTIVE)
            return;

        OpponentScore = Math.Max(0, Math.Min(newScore, TargetValue));
        UpdatedAt = now;
    }

    /// <summary>
    /// Düelloyu sonlandır ve sonucu hesapla.
    /// </summary>
    public bool Finish(DateTimeOffset now)
    {
        if (Status != DuelStatus.ACTIVE)
            return false;

        Status = DuelStatus.FINISHED;
        EndDate = now;
        Result = CalculateResult();
        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// Sonucu hesapla.
    /// </summary>
    private string CalculateResult()
    {
        bool challengerReached = ChallengerScore >= TargetValue;
        bool opponentReached = OpponentScore >= TargetValue;

        if (challengerReached && opponentReached)
        {
            // İkisi de hedefe ulaştı
            if (ChallengerScore > OpponentScore)
                return DuelResult.CHALLENGER_WIN;
            if (OpponentScore > ChallengerScore)
                return DuelResult.OPPONENT_WIN;
            return DuelResult.BOTH_WIN;
        }

        if (challengerReached)
            return DuelResult.CHALLENGER_WIN;
        
        if (opponentReached)
            return DuelResult.OPPONENT_WIN;

        // Kimse hedefe ulaşmadı - skora göre
        if (ChallengerScore > OpponentScore)
            return DuelResult.CHALLENGER_WIN;
        if (OpponentScore > ChallengerScore)
            return DuelResult.OPPONENT_WIN;

        // Eşit skor
        return DuelResult.BOTH_LOSE;
    }

    /// <summary>
    /// Rakibi dürt (günde bir kez).
    /// </summary>
    public bool Poke(Guid pokerId, DateTimeOffset now)
    {
        if (Status != DuelStatus.ACTIVE)
            return false;

        if (pokerId == ChallengerId)
        {
            // Bugün daha önce dürtmüş mü?
            if (ChallengerLastPokeAt.HasValue && 
                ChallengerLastPokeAt.Value.Date == now.Date)
                return false;

            ChallengerLastPokeAt = now;
            UpdatedAt = now;
            return true;
        }

        if (pokerId == OpponentId)
        {
            if (OpponentLastPokeAt.HasValue && 
                OpponentLastPokeAt.Value.Date == now.Date)
                return false;

            OpponentLastPokeAt = now;
            UpdatedAt = now;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Düello süresi doldu mu?
    /// </summary>
    public bool IsExpired(DateTimeOffset now) => 
        Status == DuelStatus.ACTIVE && EndDate.HasValue && now >= EndDate.Value;

    /// <summary>
    /// Kullanıcı bu düellonun tarafı mı?
    /// </summary>
    public bool IsParticipant(Guid userId) => 
        userId == ChallengerId || userId == OpponentId;
}

/// <summary>
/// Düello durumu sabitleri.
/// </summary>
public static class DuelStatus
{
    public const string WAITING = "WAITING";
    public const string ACTIVE = "ACTIVE";
    public const string FINISHED = "FINISHED";
    public const string REJECTED = "REJECTED";
    public const string EXPIRED = "EXPIRED";
}

/// <summary>
/// Düello sonucu sabitleri.
/// </summary>
public static class DuelResult
{
    public const string CHALLENGER_WIN = "CHALLENGER_WIN";
    public const string OPPONENT_WIN = "OPPONENT_WIN";
    public const string BOTH_WIN = "BOTH_WIN";
    public const string BOTH_LOSE = "BOTH_LOSE";
}

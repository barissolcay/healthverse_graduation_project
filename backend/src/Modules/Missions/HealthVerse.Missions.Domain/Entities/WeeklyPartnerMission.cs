using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Missions.Domain.Entities;

/// <summary>
/// Haftalık partner görevi entity.
/// İki arkadaşın birlikte tamamladığı ortak hedef.
/// </summary>
public sealed class WeeklyPartnerMission : Entity
{
    /// <summary>
    /// Hafta ID'si. Format: YYYY-Www (örn: 2024-W52)
    /// </summary>
    public string WeekId { get; private init; } = null!;
    
    public Guid InitiatorId { get; private init; }
    public Guid PartnerId { get; private init; }
    
    /// <summary>
    /// Aktivite tipi. NULL ise genel görev.
    /// </summary>
    public string? ActivityType { get; private init; }
    
    /// <summary>
    /// Hedef metriği: STEPS, DISTANCE, CALORIES vb.
    /// </summary>
    public string TargetMetric { get; private init; } = null!;
    
    public int TargetValue { get; private init; }
    public int InitiatorProgress { get; private set; }
    public int PartnerProgress { get; private set; }
    
    /// <summary>
    /// Görev durumu: ACTIVE, FINISHED, CANCELLED, EXPIRED
    /// </summary>
    public string Status { get; private set; } = null!;
    
    public DateTimeOffset? InitiatorLastPokeAt { get; private set; }
    public DateTimeOffset? PartnerLastPokeAt { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private WeeklyPartnerMission() { }

    public static WeeklyPartnerMission Create(
        string weekId,
        Guid initiatorId,
        Guid partnerId,
        string targetMetric = "STEPS",
        int targetValue = 100000,
        string? activityType = null)
    {
        if (string.IsNullOrWhiteSpace(weekId))
            throw new DomainException("WeeklyPartnerMission.InvalidWeekId", "WeekId cannot be empty.");

        if (initiatorId == Guid.Empty)
            throw new DomainException("WeeklyPartnerMission.InvalidInitiator", "InitiatorId cannot be empty.");

        if (partnerId == Guid.Empty)
            throw new DomainException("WeeklyPartnerMission.InvalidPartner", "PartnerId cannot be empty.");

        if (initiatorId == partnerId)
            throw new DomainException("WeeklyPartnerMission.SelfPairing", "Cannot pair with yourself.");

        if (targetValue <= 0)
            throw new DomainException("WeeklyPartnerMission.InvalidTarget", "TargetValue must be positive.");

        var now = DateTimeOffset.UtcNow;
        return new WeeklyPartnerMission
        {
            Id = Guid.NewGuid(),
            WeekId = weekId,
            InitiatorId = initiatorId,
            PartnerId = partnerId,
            ActivityType = activityType?.ToUpperInvariant(),
            TargetMetric = targetMetric.ToUpperInvariant(),
            TargetValue = targetValue,
            InitiatorProgress = 0,
            PartnerProgress = 0,
            Status = PartnerMissionStatus.ACTIVE,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Initiator ilerlemesini güncelle.
    /// </summary>
    public void UpdateInitiatorProgress(int newProgress, DateTimeOffset now)
    {
        if (Status != PartnerMissionStatus.ACTIVE)
            return;

        InitiatorProgress = Math.Max(0, Math.Min(newProgress, TargetValue));
        UpdatedAt = now;
    }

    /// <summary>
    /// Partner ilerlemesini güncelle.
    /// </summary>
    public void UpdatePartnerProgress(int newProgress, DateTimeOffset now)
    {
        if (Status != PartnerMissionStatus.ACTIVE)
            return;

        PartnerProgress = Math.Max(0, Math.Min(newProgress, TargetValue));
        UpdatedAt = now;
    }

    /// <summary>
    /// Görevi tamamla.
    /// </summary>
    public void Finish(DateTimeOffset now)
    {
        if (Status != PartnerMissionStatus.ACTIVE)
            return;

        Status = PartnerMissionStatus.FINISHED;
        UpdatedAt = now;
    }

    /// <summary>
    /// Görevi iptal et.
    /// </summary>
    public void Cancel(DateTimeOffset now)
    {
        if (Status != PartnerMissionStatus.ACTIVE)
            return;

        Status = PartnerMissionStatus.CANCELLED;
        UpdatedAt = now;
    }

    /// <summary>
    /// Hafta bitince expire et.
    /// </summary>
    public void Expire(DateTimeOffset now)
    {
        if (Status != PartnerMissionStatus.ACTIVE)
            return;

        Status = PartnerMissionStatus.EXPIRED;
        UpdatedAt = now;
    }

    /// <summary>
    /// Partneri dürt (günde 1 kez).
    /// </summary>
    public bool Poke(Guid pokerId, DateTimeOffset now)
    {
        if (Status != PartnerMissionStatus.ACTIVE)
            return false;

        if (pokerId == InitiatorId)
        {
            if (InitiatorLastPokeAt.HasValue && InitiatorLastPokeAt.Value.Date == now.Date)
                return false;

            InitiatorLastPokeAt = now;
            UpdatedAt = now;
            return true;
        }

        if (pokerId == PartnerId)
        {
            if (PartnerLastPokeAt.HasValue && PartnerLastPokeAt.Value.Date == now.Date)
                return false;

            PartnerLastPokeAt = now;
            UpdatedAt = now;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Toplam ilerleme.
    /// </summary>
    public int TotalProgress => InitiatorProgress + PartnerProgress;

    /// <summary>
    /// Görev tamamlandı mı?
    /// </summary>
    public bool IsCompleted => TotalProgress >= TargetValue;

    /// <summary>
    /// İlerleme yüzdesi (0-100).
    /// </summary>
    public int ProgressPercent => TargetValue > 0 
        ? Math.Min(100, (int)((TotalProgress * 100) / TargetValue)) 
        : 0;

    /// <summary>
    /// Kullanıcı bu görevin katılımcısı mı?
    /// </summary>
    public bool IsParticipant(Guid userId) => userId == InitiatorId || userId == PartnerId;
}

/// <summary>
/// Partner Mission durumu sabitleri.
/// </summary>
public static class PartnerMissionStatus
{
    public const string ACTIVE = "ACTIVE";
    public const string FINISHED = "FINISHED";
    public const string CANCELLED = "CANCELLED";
    public const string EXPIRED = "EXPIRED";
}

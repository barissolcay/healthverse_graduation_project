using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Missions.Domain.Entities;

/// <summary>
/// Global (dünya) görevi entity.
/// Tüm kullanıcıların birlikte katıldığı topluluk hedefi.
/// </summary>
public sealed class GlobalMission : Entity
{
    public string Title { get; private init; } = null!;
    
    /// <summary>
    /// Aktivite tipi. NULL ise genel görev.
    /// </summary>
    public string? ActivityType { get; private init; }
    
    /// <summary>
    /// Hedef metriği: STEPS, DISTANCE, CALORIES vb.
    /// </summary>
    public string TargetMetric { get; private init; } = null!;
    
    public long TargetValue { get; private init; }
    
    /// <summary>
    /// Cache: Toplam katkı değeri. Gerçek veri GlobalMissionContributions'da.
    /// </summary>
    public long CurrentValue { get; private set; }
    
    /// <summary>
    /// Görevi tamamlayanlara verilecek gizli ödül puanı.
    /// </summary>
    public int HiddenRewardPoints { get; private init; }
    
    /// <summary>
    /// Görev durumu: DRAFT, ACTIVE, FINISHED, CANCELLED
    /// </summary>
    public string Status { get; private set; } = null!;
    
    public DateTimeOffset StartDate { get; private init; }
    public DateTimeOffset EndDate { get; private init; }

    private GlobalMission() { }

    public static GlobalMission Create(
        string title,
        string targetMetric,
        long targetValue,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string? activityType = null,
        int hiddenRewardPoints = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("GlobalMission.InvalidTitle", "Title cannot be empty.");

        if (string.IsNullOrWhiteSpace(targetMetric))
            throw new DomainException("GlobalMission.InvalidMetric", "TargetMetric cannot be empty.");

        if (targetValue <= 0)
            throw new DomainException("GlobalMission.InvalidTarget", "TargetValue must be positive.");

        if (endDate <= startDate)
            throw new DomainException("GlobalMission.InvalidTimeWindow", "EndDate must be after StartDate.");

        return new GlobalMission
        {
            Id = Guid.NewGuid(),
            Title = title,
            ActivityType = activityType?.ToUpperInvariant(),
            TargetMetric = targetMetric.ToUpperInvariant(),
            TargetValue = targetValue,
            CurrentValue = 0,
            HiddenRewardPoints = hiddenRewardPoints,
            Status = MissionStatus.DRAFT,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public void Activate()
    {
        if (Status != MissionStatus.DRAFT)
            return;
        Status = MissionStatus.ACTIVE;
    }

    public void Finish()
    {
        if (Status != MissionStatus.ACTIVE)
            return;
        Status = MissionStatus.FINISHED;
    }

    public void Cancel()
    {
        if (Status == MissionStatus.FINISHED)
            return;
        Status = MissionStatus.CANCELLED;
    }

    /// <summary>
    /// Cache'i güncelle (contribution eklendiğinde).
    /// </summary>
    public void AddContribution(long amount)
    {
        if (amount <= 0 || Status != MissionStatus.ACTIVE)
            return;
        CurrentValue = Math.Min(CurrentValue + amount, TargetValue);
    }

    /// <summary>
    /// Görev tamamlandı mı?
    /// </summary>
    public bool IsCompleted => CurrentValue >= TargetValue;

    /// <summary>
    /// Görev aktif mi?
    /// </summary>
    public bool IsActive => Status == MissionStatus.ACTIVE;

    /// <summary>
    /// İlerleme yüzdesi (0-100).
    /// </summary>
    public int ProgressPercent => TargetValue > 0 
        ? Math.Min(100, (int)((CurrentValue * 100) / TargetValue)) 
        : 0;
}

/// <summary>
/// Global Mission durumu sabitleri.
/// </summary>
public static class MissionStatus
{
    public const string DRAFT = "DRAFT";
    public const string ACTIVE = "ACTIVE";
    public const string FINISHED = "FINISHED";
    public const string CANCELLED = "CANCELLED";
}

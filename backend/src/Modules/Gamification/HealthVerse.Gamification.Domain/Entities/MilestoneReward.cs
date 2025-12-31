using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Gamification.Domain.Entities;

/// <summary>
/// Milestone ödül entity.
/// Belirli hedeflere ulaşıldığında verilen ödüller (rozet, ünvan, freeze).
/// </summary>
public sealed class MilestoneReward : Entity
{
    /// <summary>
    /// Milestone kodu: STREAK_7, TASKS_100, DUEL_WIN_10, vb.
    /// </summary>
    public string Code { get; private init; } = null!;
    
    /// <summary>
    /// Görüntüleme adı.
    /// </summary>
    public string Title { get; private init; } = null!;
    
    /// <summary>
    /// Açıklama.
    /// </summary>
    public string Description { get; private init; } = null!;
    
    /// <summary>
    /// Ödül tipi: BADGE, TITLE, FREEZE
    /// </summary>
    public string RewardType { get; private init; } = null!;
    
    /// <summary>
    /// Metrik: STREAK_COUNT, TASK_COUNT, DUEL_WIN, STEPS_TOTAL, vb.
    /// </summary>
    public string Metric { get; private init; } = null!;
    
    /// <summary>
    /// Hedef değer.
    /// </summary>
    public int TargetValue { get; private init; }
    
    /// <summary>
    /// Ödül olarak verilen freeze sayısı (FREEZE tipi için).
    /// </summary>
    public int FreezeReward { get; private init; }
    
    /// <summary>
    /// Ödül olarak verilen puan.
    /// </summary>
    public int PointsReward { get; private init; }
    
    /// <summary>
    /// İkon adı (badge için).
    /// </summary>
    public string? IconName { get; private init; }
    
    /// <summary>
    /// Sıralama (gösterim için).
    /// </summary>
    public int DisplayOrder { get; private init; }
    
    public bool IsActive { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }

    private MilestoneReward() { }

    public static MilestoneReward Create(
        string code,
        string title,
        string description,
        string rewardType,
        string metric,
        int targetValue,
        int freezeReward = 0,
        int pointsReward = 0,
        string? iconName = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("MilestoneReward.InvalidCode", "Code cannot be empty.");

        if (targetValue <= 0)
            throw new DomainException("MilestoneReward.InvalidTarget", "TargetValue must be positive.");

        return new MilestoneReward
        {
            Id = Guid.NewGuid(),
            Code = code.ToUpperInvariant(),
            Title = title,
            Description = description,
            RewardType = rewardType.ToUpperInvariant(),
            Metric = metric.ToUpperInvariant(),
            TargetValue = targetValue,
            FreezeReward = freezeReward,
            PointsReward = pointsReward,
            IconName = iconName,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

/// <summary>
/// Kullanıcı milestone başarısı entity.
/// Kullanıcının kazandığı milestone'ları takip eder.
/// </summary>
public sealed class UserMilestone : Entity
{
    public Guid UserId { get; private init; }
    public Guid MilestoneRewardId { get; private init; }
    
    /// <summary>
    /// Kazanılma zamanı.
    /// </summary>
    public DateTimeOffset AchievedAt { get; private init; }
    
    /// <summary>
    /// Ödül toplandı mı?
    /// </summary>
    public bool IsClaimed { get; private set; }
    public DateTimeOffset? ClaimedAt { get; private set; }

    private UserMilestone() { }

    public static UserMilestone Create(Guid userId, Guid milestoneRewardId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserMilestone.InvalidUser", "UserId cannot be empty.");

        if (milestoneRewardId == Guid.Empty)
            throw new DomainException("UserMilestone.InvalidMilestone", "MilestoneRewardId cannot be empty.");

        return new UserMilestone
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MilestoneRewardId = milestoneRewardId,
            AchievedAt = DateTimeOffset.UtcNow,
            IsClaimed = false
        };
    }

    public void Claim()
    {
        if (IsClaimed) return;
        IsClaimed = true;
        ClaimedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Milestone ödül tipi sabitleri.
/// </summary>
public static class MilestoneRewardType
{
    public const string BADGE = "BADGE";
    public const string TITLE = "TITLE";
    public const string FREEZE = "FREEZE";
}

/// <summary>
/// Milestone metrik sabitleri.
/// </summary>
public static class MilestoneMetric
{
    public const string STREAK_COUNT = "STREAK_COUNT";           // Mevcut seri uzunluğu
    public const string STREAK_MAX = "STREAK_MAX";               // En uzun seri
    public const string TASK_COMPLETED = "TASK_COMPLETED";       // Tamamlanan görev sayısı
    public const string GOAL_COMPLETED = "GOAL_COMPLETED";       // Tamamlanan hedef sayısı
    public const string DUEL_WIN = "DUEL_WIN";                   // Kazanılan düello sayısı
    public const string DUEL_TOTAL = "DUEL_TOTAL";               // Toplam düello sayısı
    public const string PARTNER_MISSION_COMPLETED = "PARTNER_MISSION_COMPLETED"; // Tamamlanan ortak görev
    public const string GLOBAL_MISSION_COMPLETED = "GLOBAL_MISSION_COMPLETED";   // Katılınan global görev
    public const string STEPS_TOTAL = "STEPS_TOTAL";             // Toplam adım
    public const string POINTS_TOTAL = "POINTS_TOTAL";           // Toplam puan
}

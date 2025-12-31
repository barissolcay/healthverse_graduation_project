using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Tasks.Domain.Entities;

/// <summary>
/// Görev şablonu entity.
/// Catalog/lookup tablo - seed data ile doldurulur.
/// </summary>
public sealed class TaskTemplate : Entity
{
    public string Title { get; private init; } = null!;
    public string? Description { get; private init; }
    
    /// <summary>
    /// Görev kategorisi. Örnek: "Günlük", "Haftalık", "Meydan Okuma"
    /// </summary>
    public string? Category { get; private init; }
    
    /// <summary>
    /// Aktivite tipi. NULL ise genel görev (ilgi alanı olmayan kullanıcılara verilir).
    /// Örnek: RUNNING, WALKING, CYCLING, SWIMMING
    /// </summary>
    public string? ActivityType { get; private init; }
    
    /// <summary>
    /// Hedef metriği: STEPS, DURATION, CALORIES, DISTANCE vb.
    /// </summary>
    public string TargetMetric { get; private init; } = null!;
    
    public int TargetValue { get; private init; }
    public int RewardPoints { get; private init; }
    
    /// <summary>
    /// Görev tamamlandığında verilecek rozet (opsiyonel).
    /// </summary>
    public string? BadgeId { get; private init; }
    
    /// <summary>
    /// Görev tamamlandığında verilecek ünvan (opsiyonel).
    /// </summary>
    public string? TitleId { get; private init; }
    
    public bool IsActive { get; private set; }

    private TaskTemplate() { }

    public static TaskTemplate Create(
        string title,
        string targetMetric,
        int targetValue,
        int rewardPoints,
        string? description = null,
        string? category = null,
        string? activityType = null,
        string? badgeId = null,
        string? titleId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("TaskTemplate.InvalidTitle", "Title cannot be empty.");

        if (string.IsNullOrWhiteSpace(targetMetric))
            throw new DomainException("TaskTemplate.InvalidMetric", "TargetMetric cannot be empty.");

        if (targetValue <= 0)
            throw new DomainException("TaskTemplate.InvalidTarget", "TargetValue must be positive.");

        if (rewardPoints <= 0)
            throw new DomainException("TaskTemplate.InvalidReward", "RewardPoints must be positive.");

        return new TaskTemplate
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Category = category,
            ActivityType = activityType?.ToUpperInvariant(),
            TargetMetric = targetMetric.ToUpperInvariant(),
            TargetValue = targetValue,
            RewardPoints = rewardPoints,
            BadgeId = badgeId,
            TitleId = titleId,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}

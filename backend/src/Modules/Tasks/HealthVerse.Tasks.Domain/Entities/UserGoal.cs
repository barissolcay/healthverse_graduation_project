using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Tasks.Domain.Entities;

/// <summary>
/// Kullanıcının kişisel hedefi.
/// 
/// Kurallar:
/// - CurrentValue TargetValue'yu geçemez
/// - CurrentValue >= TargetValue olunca otomatik CompletedAt set edilir
/// - Başarısız hedefler silinir, tamamlananlar saklanır
/// </summary>
public sealed class UserGoal : Entity
{
    public Guid UserId { get; private init; }
    
    public string Title { get; private init; } = null!;
    public string? Description { get; private init; }
    
    /// <summary>
    /// Aktivite tipi: RUNNING, WALKING, CYCLING, SWIMMING vb.
    /// </summary>
    public string? ActivityType { get; private init; }
    
    /// <summary>
    /// Hedef metriği: STEPS, DURATION, CALORIES, DISTANCE vb.
    /// </summary>
    public string TargetMetric { get; private init; } = null!;
    
    public int TargetValue { get; private init; }
    public int CurrentValue { get; private set; }
    
    public DateTimeOffset ValidUntil { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private UserGoal() { }

    public static UserGoal Create(
        Guid userId,
        string title,
        string targetMetric,
        int targetValue,
        DateTimeOffset validUntil,
        string? description = null,
        string? activityType = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserGoal.InvalidUser", "UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("UserGoal.InvalidTitle", "Title cannot be empty.");

        if (string.IsNullOrWhiteSpace(targetMetric))
            throw new DomainException("UserGoal.InvalidMetric", "TargetMetric cannot be empty.");

        if (targetValue <= 0)
            throw new DomainException("UserGoal.InvalidTarget", "TargetValue must be positive.");

        var now = DateTimeOffset.UtcNow;
        
        if (validUntil <= now)
            throw new DomainException("UserGoal.InvalidDeadline", "ValidUntil must be in the future.");

        return new UserGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Description = description,
            ActivityType = activityType?.ToUpperInvariant(),
            TargetMetric = targetMetric.ToUpperInvariant(),
            TargetValue = targetValue,
            CurrentValue = 0,
            ValidUntil = validUntil,
            CreatedAt = now
        };
    }

    /// <summary>
    /// İlerleme günceller. TargetValue'yu geçerse otomatik tamamlanır.
    /// </summary>
    public bool UpdateProgress(int newValue)
    {
        if (CompletedAt != null)
            return false; // Zaten tamamlandı

        // CurrentValue TargetValue'yu geçemez
        CurrentValue = Math.Min(newValue, TargetValue);

        // Hedef tamamlandıysa otomatik complete
        if (CurrentValue >= TargetValue)
        {
            CompletedAt = DateTimeOffset.UtcNow;
            return true; // Tamamlandı
        }

        return false;
    }

    /// <summary>
    /// Hedef tamamlandı mı?
    /// </summary>
    public bool IsCompleted => CompletedAt != null;

    /// <summary>
    /// Hedef süresi doldu mu?
    /// </summary>
    public bool IsExpired(DateTimeOffset now) => ValidUntil <= now && !IsCompleted;

    /// <summary>
    /// İlerleme yüzdesi (0-100).
    /// </summary>
    public int ProgressPercent => TargetValue > 0 
        ? Math.Min(100, (int)((CurrentValue / (double)TargetValue) * 100)) 
        : 0;
}

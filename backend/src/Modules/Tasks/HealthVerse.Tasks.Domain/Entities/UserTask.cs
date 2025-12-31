using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Tasks.Domain.Entities;

/// <summary>
/// Kullanıcıya atanmış görev entity.
/// 
/// Akış:
/// ACTIVE -> COMPLETED (hedef sağlandı, puan ledger'a yazıldı)
/// COMPLETED -> REWARD_CLAIMED (UI onayı, puan tekrar verilmez)
/// ACTIVE -> FAILED (süre doldu veya iptal)
/// </summary>
public sealed class UserTask : Entity
{
    public Guid UserId { get; private init; }
    public Guid TemplateId { get; private init; }
    
    public int CurrentValue { get; private set; }
    
    /// <summary>
    /// Görev durumu: ACTIVE, COMPLETED, REWARD_CLAIMED, FAILED
    /// </summary>
    public string Status { get; private set; } = null!;
    
    public DateTimeOffset ValidUntil { get; private init; }
    public DateTimeOffset AssignedAt { get; private init; }
    
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? RewardClaimedAt { get; private set; }
    public DateTimeOffset? FailedAt { get; private set; }

    private UserTask() { }

    public static UserTask Assign(
        Guid userId,
        Guid templateId,
        DateTimeOffset validUntil)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserTask.InvalidUser", "UserId cannot be empty.");

        if (templateId == Guid.Empty)
            throw new DomainException("UserTask.InvalidTemplate", "TemplateId cannot be empty.");

        var now = DateTimeOffset.UtcNow;
        
        if (validUntil <= now)
            throw new DomainException("UserTask.InvalidDeadline", "ValidUntil must be in the future.");

        // Max 7 gün kuralı
        if (validUntil > now.AddDays(7))
            throw new DomainException("UserTask.DeadlineTooFar", "ValidUntil cannot exceed 7 days.");

        return new UserTask
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = templateId,
            CurrentValue = 0,
            Status = UserTaskStatus.ACTIVE,
            ValidUntil = validUntil,
            AssignedAt = now
        };
    }

    /// <summary>
    /// İlerleme günceller. TargetValue'yu geçerse otomatik tamamlanır.
    /// </summary>
    public bool UpdateProgress(int newValue, int targetValue)
    {
        if (Status != UserTaskStatus.ACTIVE)
            return false;

        // CurrentValue TargetValue'yu geçemez
        CurrentValue = Math.Min(newValue, targetValue);

        // Hedef tamamlandıysa otomatik complete
        if (CurrentValue >= targetValue)
        {
            Status = UserTaskStatus.COMPLETED;
            CompletedAt = DateTimeOffset.UtcNow;
            return true; // Tamamlandı
        }

        return false;
    }

    /// <summary>
    /// Kullanıcı UI'da ödülü topladığını onaylar.
    /// Puan zaten verildi, bu sadece UI animasyonu için.
    /// </summary>
    public bool ClaimReward()
    {
        if (Status != UserTaskStatus.COMPLETED)
            return false;

        Status = UserTaskStatus.REWARD_CLAIMED;
        RewardClaimedAt = DateTimeOffset.UtcNow;
        return true;
    }

    /// <summary>
    /// Süre doldu veya iptal edildi.
    /// </summary>
    public void MarkAsFailed()
    {
        if (Status != UserTaskStatus.ACTIVE)
            return;

        Status = UserTaskStatus.FAILED;
        FailedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Görevin süresi dolmuş mu?
    /// </summary>
    public bool IsExpired(DateTimeOffset now) => ValidUntil <= now && Status == UserTaskStatus.ACTIVE;
}

/// <summary>
/// Görev durumu sabitleri.
/// </summary>
public static class UserTaskStatus
{
    public const string ACTIVE = "ACTIVE";
    public const string COMPLETED = "COMPLETED";
    public const string REWARD_CLAIMED = "REWARD_CLAIMED";
    public const string FAILED = "FAILED";
}

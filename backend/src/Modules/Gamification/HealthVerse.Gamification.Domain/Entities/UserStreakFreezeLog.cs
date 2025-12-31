using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Gamification.Domain.Entities;

/// <summary>
/// Dondurma hakkı kullanım geçmişini tutar.
/// Aynı kullanıcı aynı gün birden fazla dondurma hakkı kullanamaz.
/// </summary>
public sealed class UserStreakFreezeLog : Entity
{
    public Guid UserId { get; private init; }
    
    /// <summary>
    /// Dondurma hakkının kullanıldığı gün (TR timezone bazlı).
    /// </summary>
    public DateOnly UsedDate { get; private init; }
    
    /// <summary>
    /// O andaki seri sayısı (bilgi amaçlı).
    /// </summary>
    public int StreakCountAtTime { get; private init; }

    /// <summary>
    /// Kayıt oluşturulma zamanı.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private init; }

    private UserStreakFreezeLog() { }

    public static UserStreakFreezeLog Create(Guid userId, DateOnly usedDate, int streakCountAtTime)
    {
        if (streakCountAtTime < 0)
            throw new DomainException("UserStreakFreezeLog.NegativeStreak", "Streak count cannot be negative.");

        return new UserStreakFreezeLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedDate = usedDate,
            StreakCountAtTime = streakCountAtTime,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

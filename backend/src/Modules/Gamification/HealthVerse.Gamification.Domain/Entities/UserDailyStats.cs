using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Gamification.Domain.Entities;

/// <summary>
/// Kullanıcının günlük adım ve puan özetlerini tutar.
/// Composite Primary Key: (UserId, LogDate)
/// </summary>
public sealed class UserDailyStats
{
    public Guid UserId { get; private init; }
    public DateOnly LogDate { get; private init; }

    public int DailySteps { get; private set; }
    public int DailyPoints { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private UserDailyStats() { }

    public static UserDailyStats Create(Guid userId, DateOnly logDate, int dailySteps = 0, int dailyPoints = 0)
    {
        if (dailySteps < 0)
            throw new DomainException("UserDailyStats.NegativeSteps", "Daily steps cannot be negative.");

        if (dailyPoints < 0)
            throw new DomainException("UserDailyStats.NegativePoints", "Daily points cannot be negative.");

        return new UserDailyStats
        {
            UserId = userId,
            LogDate = logDate,
            DailySteps = dailySteps,
            DailyPoints = dailyPoints,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Günlük adım sayısını günceller (overwrite yaklaşımı).
    /// </summary>
    public void UpdateSteps(int newSteps)
    {
        if (newSteps < 0)
            throw new DomainException("UserDailyStats.NegativeSteps", "Daily steps cannot be negative.");

        DailySteps = newSteps;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Günlük puan toplamını günceller.
    /// </summary>
    public void UpdatePoints(int newPoints)
    {
        if (newPoints < 0)
            throw new DomainException("UserDailyStats.NegativePoints", "Daily points cannot be negative.");

        DailyPoints = newPoints;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Günlük puana ekleme yapar (delta yaklaşımı).
    /// </summary>
    public void AddPoints(int points)
    {
        if (DailyPoints + points < 0)
            throw new DomainException("UserDailyStats.NegativePoints", "Daily points cannot be negative.");

        DailyPoints += points;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

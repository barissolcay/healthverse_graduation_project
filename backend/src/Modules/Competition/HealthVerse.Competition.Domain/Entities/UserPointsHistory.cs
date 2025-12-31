using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Competition.Domain.Entities;

/// <summary>
/// Kullanıcının haftalık/aylık puan geçmişini saklar.
/// Haftalık lig sıralaması bittiğinde veya aylık dönem kapandığında kaydedilir.
/// </summary>
public sealed class UserPointsHistory : Entity
{
    public Guid UserId { get; private init; }
    
    /// <summary>
    /// Dönem tipi: "WEEKLY" veya "MONTHLY"
    /// </summary>
    public string PeriodType { get; private init; } = null!;
    
    /// <summary>
    /// Dönem kimliği: "2025-W03" (haftalık) veya "2025-01" (aylık)
    /// </summary>
    public string PeriodId { get; private init; } = null!;
    
    /// <summary>
    /// O dönemde kazanılan puan.
    /// </summary>
    public int Points { get; private init; }
    
    /// <summary>
    /// O dönemdeki sıralama (oda içinde).
    /// </summary>
    public int? LeagueRank { get; private init; }
    
    /// <summary>
    /// O dönemdeki lig.
    /// </summary>
    public string? TierAtTime { get; private init; }
    
    /// <summary>
    /// Sonuç: PROMOTED, DEMOTED, STAYED
    /// </summary>
    public string? Result { get; private init; }

    /// <summary>
    /// Kayıt oluşturulma zamanı.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private init; }

    private UserPointsHistory() { }

    public static UserPointsHistory CreateWeekly(
        Guid userId,
        string weekId,
        int points,
        int rank,
        string tier,
        string result)
    {
        return new UserPointsHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PeriodType = "WEEKLY",
            PeriodId = weekId,
            Points = points,
            LeagueRank = rank,
            TierAtTime = tier,
            Result = result,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static UserPointsHistory CreateMonthly(
        Guid userId,
        string monthId,
        int points)
    {
        return new UserPointsHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PeriodType = "MONTHLY",
            PeriodId = monthId,
            Points = points,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

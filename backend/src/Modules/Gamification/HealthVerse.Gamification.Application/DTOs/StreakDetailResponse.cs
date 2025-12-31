namespace HealthVerse.Gamification.Application.DTOs;

/// <summary>
/// Streak detay bilgisi DTO'su.
/// GET /api/users/{id}/streak endpoint'i için response.
/// </summary>
public sealed class StreakDetailResponse
{
    /// <summary>
    /// Mevcut seri sayısı.
    /// </summary>
    public int StreakCount { get; init; }

    /// <summary>
    /// En uzun seri sayısı.
    /// </summary>
    public int LongestStreakCount { get; init; }

    /// <summary>
    /// Bugünkü adım sayısı.
    /// </summary>
    public int TodaySteps { get; init; }

    /// <summary>
    /// Streak için gereken minimum adım (3000).
    /// </summary>
    public int StreakThreshold { get; init; } = 3000;

    /// <summary>
    /// Bugünkü ilerleme yüzdesi (0-100).
    /// Maksimum 100, 3000 adıma ulaşıldığında.
    /// </summary>
    public int TodayProgressPercent { get; init; }

    /// <summary>
    /// Kalan dondurma hakkı sayısı.
    /// </summary>
    public int FreezeInventory { get; init; }

    /// <summary>
    /// Bugün streak korunabilir mi? (TodaySteps >= 3000)
    /// </summary>
    public bool StreakSecuredToday { get; init; }

    /// <summary>
    /// Son seri tarihi (varsa).
    /// </summary>
    public DateOnly? LastStreakDate { get; init; }
}

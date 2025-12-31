namespace HealthVerse.Gamification.Domain.Services;

/// <summary>
/// Domain service for managing user streaks.
/// 
/// Streak Kuralları:
/// - 3000+ adım: Streak korunur ve artar (+1)
/// - 3000 altı + Freeze varsa: Freeze kullanılır, streak sabit kalır
/// - 3000 altı + Freeze yoksa: Streak sıfırlanır
/// </summary>
public class StreakService
{
    public const int StreakThreshold = 3000;

    /// <summary>
    /// Günlük adım sayısına göre streak durumunu değerlendirir.
    /// </summary>
    /// <param name="dailySteps">Günlük adım sayısı</param>
    /// <param name="currentStreakCount">Mevcut streak sayısı</param>
    /// <param name="freezeInventory">Mevcut dondurma hakkı sayısı</param>
    /// <returns>Streak sonucu</returns>
    public StreakResult EvaluateStreak(int dailySteps, int currentStreakCount, int freezeInventory)
    {
        // Kural 1: 3000+ adım atıldıysa streak artar
        if (dailySteps >= StreakThreshold)
        {
            return new StreakResult
            {
                NewStreakCount = currentStreakCount + 1,
                FreezeUsed = false,
                StreakLost = false,
                Action = StreakAction.Increment
            };
        }

        // Kural 2: 3000 altı + Freeze varsa freeze kullanılır
        if (freezeInventory > 0)
        {
            return new StreakResult
            {
                NewStreakCount = currentStreakCount, // Sabit kalır
                FreezeUsed = true,
                StreakLost = false,
                Action = StreakAction.Frozen
            };
        }

        // Kural 3: 3000 altı + Freeze yoksa streak sıfırlanır
        return new StreakResult
        {
            NewStreakCount = 0,
            FreezeUsed = false,
            StreakLost = currentStreakCount > 0, // Sadece önceden streak varsa "kayıp" sayılır
            Action = StreakAction.Lost
        };
    }

    /// <summary>
    /// Verilen adım sayısının streak korumak için yeterli olup olmadığını kontrol eder.
    /// </summary>
    public bool MeetsStreakThreshold(int dailySteps)
    {
        return dailySteps >= StreakThreshold;
    }
}

/// <summary>
/// Streak değerlendirme sonucu.
/// </summary>
public class StreakResult
{
    public int NewStreakCount { get; init; }
    public bool FreezeUsed { get; init; }
    public bool StreakLost { get; init; }
    public StreakAction Action { get; init; }
}

/// <summary>
/// Streak aksiyonu türleri.
/// </summary>
public enum StreakAction
{
    /// <summary>Streak arttı (3000+ adım atıldı)</summary>
    Increment,
    
    /// <summary>Streak donduruldu (freeze kullanıldı)</summary>
    Frozen,
    
    /// <summary>Streak kaybedildi (3000 altı + freeze yok)</summary>
    Lost
}

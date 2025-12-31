namespace HealthVerse.Missions.Domain.Entities;

/// <summary>
/// Global Mission katılımcısı - composite PK (MissionId, UserId).
/// Her kullanıcının o görevdeki toplam katkısını ve ödül durumunu tutar.
/// </summary>
public sealed class GlobalMissionParticipant
{
    public Guid MissionId { get; private init; }
    public Guid UserId { get; private init; }
    
    /// <summary>
    /// Bu kullanıcının toplam katkı değeri (cache).
    /// </summary>
    public long ContributionValue { get; private set; }
    
    /// <summary>
    /// Görev bittiğinde ödül alındı mı?
    /// </summary>
    public bool IsRewardClaimed { get; private set; }
    
    public DateTimeOffset JoinedAt { get; private init; }

    private GlobalMissionParticipant() { }

    public static GlobalMissionParticipant Create(Guid missionId, Guid userId)
    {
        if (missionId == Guid.Empty)
            throw new ArgumentException("MissionId cannot be empty.", nameof(missionId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        return new GlobalMissionParticipant
        {
            MissionId = missionId,
            UserId = userId,
            ContributionValue = 0,
            IsRewardClaimed = false,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Katkı ekle (cache güncelleme).
    /// </summary>
    public void AddContribution(long amount)
    {
        if (amount <= 0) return;
        ContributionValue += amount;
    }

    /// <summary>
    /// Ödülü topla.
    /// </summary>
    public bool ClaimReward()
    {
        if (IsRewardClaimed || ContributionValue <= 0)
            return false;
        IsRewardClaimed = true;
        return true;
    }
}

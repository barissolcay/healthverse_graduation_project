namespace HealthVerse.Missions.Domain.Entities;

/// <summary>
/// Haftalık partner görevi slot tablosu.
/// Her kullanıcı her hafta sadece bir partner görevi olabilir.
/// Composite PK: (WeekId, UserId)
/// </summary>
public sealed class WeeklyPartnerMissionSlot
{
    /// <summary>
    /// Hafta ID'si. Format: YYYY-Www (örn: 2024-W52)
    /// </summary>
    public string WeekId { get; private init; } = null!;
    
    public Guid UserId { get; private init; }
    public Guid MissionId { get; private init; }

    private WeeklyPartnerMissionSlot() { }

    public static WeeklyPartnerMissionSlot Create(string weekId, Guid userId, Guid missionId)
    {
        if (string.IsNullOrWhiteSpace(weekId))
            throw new ArgumentException("WeekId cannot be empty.", nameof(weekId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (missionId == Guid.Empty)
            throw new ArgumentException("MissionId cannot be empty.", nameof(missionId));

        return new WeeklyPartnerMissionSlot
        {
            WeekId = weekId,
            UserId = userId,
            MissionId = missionId
        };
    }
}

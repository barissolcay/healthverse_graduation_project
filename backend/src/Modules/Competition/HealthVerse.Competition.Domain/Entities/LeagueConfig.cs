namespace HealthVerse.Competition.Domain.Entities;

/// <summary>
/// League configuration entity for tier rules.
/// This is a catalog/lookup table with seed data.
/// TierName is the primary key (string, not UUID).
/// </summary>
public sealed class LeagueConfig
{
    /// <summary>
    /// Tier name (Primary Key). Examples: ISINMA, ANTRENMAN, TEMPO, etc.
    /// </summary>
    public string TierName { get; private init; } = null!;

    /// <summary>
    /// Tier order (1 = lowest, 7 = highest).
    /// Used for determining promote/demote logic.
    /// </summary>
    public int TierOrder { get; private init; }

    /// <summary>
    /// Percentage of users promoted to next tier at week end.
    /// 0 for highest tier (SAMPIYON).
    /// </summary>
    public int PromotePercentage { get; private init; }

    /// <summary>
    /// Percentage of users demoted to previous tier at week end.
    /// 0 for lowest tier (ISINMA).
    /// </summary>
    public int DemotePercentage { get; private init; }

    /// <summary>
    /// Minimum room size for this tier.
    /// </summary>
    public int MinRoomSize { get; private init; }

    /// <summary>
    /// Maximum room size for this tier.
    /// </summary>
    public int MaxRoomSize { get; private init; }

    private LeagueConfig() { }

    /// <summary>
    /// Creates a new LeagueConfig. Used for seed data.
    /// </summary>
    public static LeagueConfig Create(
        string tierName,
        int tierOrder,
        int promotePercentage,
        int demotePercentage,
        int minRoomSize = 10,
        int maxRoomSize = 20)
    {
        if (string.IsNullOrWhiteSpace(tierName))
            throw new ArgumentException("TierName cannot be empty.", nameof(tierName));

        if (tierOrder <= 0)
            throw new ArgumentException("TierOrder must be positive.", nameof(tierOrder));

        if (promotePercentage < 0 || promotePercentage > 100)
            throw new ArgumentException("PromotePercentage must be between 0 and 100.", nameof(promotePercentage));

        if (demotePercentage < 0 || demotePercentage > 100)
            throw new ArgumentException("DemotePercentage must be between 0 and 100.", nameof(demotePercentage));

        if (promotePercentage + demotePercentage > 100)
            throw new ArgumentException("PromotePercentage + DemotePercentage cannot exceed 100.");

        if (minRoomSize <= 0)
            throw new ArgumentException("MinRoomSize must be positive.", nameof(minRoomSize));

        if (maxRoomSize < minRoomSize)
            throw new ArgumentException("MaxRoomSize must be >= MinRoomSize.", nameof(maxRoomSize));

        return new LeagueConfig
        {
            TierName = tierName.ToUpperInvariant(),
            TierOrder = tierOrder,
            PromotePercentage = promotePercentage,
            DemotePercentage = demotePercentage,
            MinRoomSize = minRoomSize,
            MaxRoomSize = maxRoomSize
        };
    }

    /// <summary>
    /// Checks if this is the lowest tier (no demotion possible).
    /// </summary>
    public bool IsLowestTier => DemotePercentage == 0;

    /// <summary>
    /// Checks if this is the highest tier (no promotion possible).
    /// </summary>
    public bool IsHighestTier => PromotePercentage == 0;
}

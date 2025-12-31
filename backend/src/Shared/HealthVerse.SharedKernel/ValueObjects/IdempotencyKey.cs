using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.SharedKernel.ValueObjects;

/// <summary>
/// Idempotency key value object for ensuring operations are processed exactly once.
/// Provides factory methods for standard key formats.
/// </summary>
public sealed class IdempotencyKey : ValueObject
{
    public string Value { get; }

    private IdempotencyKey(string value)
    {
        Value = value;
    }

    public static IdempotencyKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("IdempotencyKey.Empty", "IdempotencyKey cannot be empty.");

        if (value.Length > 200)
            throw new DomainException("IdempotencyKey.TooLong", "IdempotencyKey cannot exceed 200 characters.");

        return new IdempotencyKey(value);
    }

    // ===== Factory Methods for Standard Formats =====

    /// <summary>
    /// Creates key for daily step points: STEPS_DAILY:{UserId}:{LogDate}
    /// </summary>
    public static IdempotencyKey ForDailySteps(Guid userId, DateOnly logDate)
        => new($"STEPS_DAILY:{userId}:{logDate:yyyy-MM-dd}");

    /// <summary>
    /// Creates key for weekly partner mission reward: WPM_REWARD:{WeekId}:{UserId}
    /// </summary>
    public static IdempotencyKey ForWeeklyPartnerReward(string weekId, Guid userId)
        => new($"WPM_REWARD:{weekId}:{userId}");

    /// <summary>
    /// Creates key for global mission reward: GM_REWARD:{MissionId}:{UserId}
    /// </summary>
    public static IdempotencyKey ForGlobalMissionReward(int missionId, Guid userId)
        => new($"GM_REWARD:{missionId}:{userId}");

    /// <summary>
    /// Creates key for league weekly reward: LEAGUE_REWARD:{WeekId}:{UserId}
    /// </summary>
    public static IdempotencyKey ForLeagueReward(string weekId, Guid userId)
        => new($"LEAGUE_REWARD:{weekId}:{userId}");

    /// <summary>
    /// Creates key for milestone reward: MILESTONE:{MilestoneRewardId}:{UserId}
    /// </summary>
    public static IdempotencyKey ForMilestoneReward(int milestoneRewardId, Guid userId)
        => new($"MILESTONE:{milestoneRewardId}:{userId}");

    /// <summary>
    /// Creates key for task completion reward: TASK_REWARD:{UserTaskId}
    /// </summary>
    public static IdempotencyKey ForTaskReward(Guid userTaskId)
        => new($"TASK_REWARD:{userTaskId}");

    /// <summary>
    /// Creates key for correction transaction: CORRECTION:{OriginalTransactionId}
    /// </summary>
    public static IdempotencyKey ForCorrection(Guid originalTransactionId)
        => new($"CORRECTION:{originalTransactionId}");

    /// <summary>
    /// Creates key for global mission contribution: GM_CONTRIB:{MissionId}:{UserId}:{ContribDate}
    /// </summary>
    public static IdempotencyKey ForGlobalMissionContribution(int missionId, Guid userId, DateOnly contribDate)
        => new($"GM_CONTRIB:{missionId}:{userId}:{contribDate:yyyy-MM-dd}");

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(IdempotencyKey key) => key.Value;
}

using HealthVerse.SharedKernel.Domain;
using HealthVerse.SharedKernel.ValueObjects;

namespace HealthVerse.Gamification.Domain.Entities;

/// <summary>
/// Point transaction entity for the ledger system.
/// This is an append-only entity - no updates allowed.
/// </summary>
public sealed class PointTransaction : Entity
{
    public Guid UserId { get; private init; }
    
    /// <summary>
    /// Amount of points. Positive = earned, Negative = correction/penalty.
    /// Cannot be zero.
    /// </summary>
    public long Amount { get; private init; }
    
    /// <summary>
    /// Source of points (e.g., 'STEPS', 'TASK', 'PARTNER_MISSION', etc.)
    /// </summary>
    public string SourceType { get; private init; } = null!;
    
    /// <summary>
    /// Optional reference to the source entity.
    /// </summary>
    public string? SourceIdText { get; private init; }
    
    /// <summary>
    /// Unique key to prevent duplicate processing.
    /// </summary>
    public IdempotencyKey IdempotencyKey { get; private init; } = null!;
    
    public string? Description { get; private init; }
    public string? Metadata { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }

    private PointTransaction() { }

    public static PointTransaction Create(
        Guid userId,
        long amount,
        string sourceType,
        IdempotencyKey idempotencyKey,
        string? sourceIdText = null,
        string? description = null,
        string? metadata = null)
    {
        if (amount == 0)
            throw new DomainException("PointTransaction.ZeroAmount", "Amount cannot be zero.");

        if (string.IsNullOrWhiteSpace(sourceType))
            throw new DomainException("PointTransaction.EmptySourceType", "SourceType cannot be empty.");

        return new PointTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            SourceType = sourceType,
            SourceIdText = sourceIdText,
            IdempotencyKey = idempotencyKey,
            Description = description,
            Metadata = metadata,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    // ===== Factory Methods for Common Scenarios =====

    public static PointTransaction FromDailySteps(
        Guid userId,
        int points,
        DateOnly logDate,
        int dailySteps)
    {
        return Create(
            userId,
            points,
            "STEPS",
            IdempotencyKey.ForDailySteps(userId, logDate),
            sourceIdText: logDate.ToString("yyyy-MM-dd"),
            description: $"Daily step points: {dailySteps} steps → {points} points");
    }

    public static PointTransaction FromTaskCompletion(
        Guid userId,
        int rewardPoints,
        Guid userTaskId,
        string taskTitle)
    {
        return Create(
            userId,
            rewardPoints,
            "TASK",
            IdempotencyKey.ForTaskReward(userTaskId),
            sourceIdText: userTaskId.ToString(),
            description: $"Task completed: {taskTitle}");
    }

    public static PointTransaction FromCorrection(
        Guid userId,
        long correctionAmount,
        Guid originalTransactionId,
        string reason)
    {
        return Create(
            userId,
            correctionAmount,
            "SYSTEM_CORRECTION",
            IdempotencyKey.ForCorrection(originalTransactionId),
            sourceIdText: originalTransactionId.ToString(),
            description: reason);
    }
}

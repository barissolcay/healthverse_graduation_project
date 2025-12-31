using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Missions.Domain.Entities;

/// <summary>
/// Global Mission katkı kaydı - append-only ledger.
/// IdempotencyKey ile aynı olayın iki kez yazılması engellenir.
/// </summary>
public sealed class GlobalMissionContribution : Entity
{
    public Guid MissionId { get; private init; }
    public Guid UserId { get; private init; }
    
    /// <summary>
    /// Katkı miktarı (TargetMetric biriminde).
    /// </summary>
    public long Amount { get; private init; }
    
    /// <summary>
    /// Tekilleştirme anahtarı.
    /// Örnek: "STEPS:{UserId}:{Date}:{SyncSessionId}"
    /// </summary>
    public string IdempotencyKey { get; private init; } = null!;
    
    public DateTimeOffset CreatedAt { get; private init; }

    private GlobalMissionContribution() { }

    public static GlobalMissionContribution Create(
        Guid missionId,
        Guid userId,
        long amount,
        string idempotencyKey)
    {
        if (missionId == Guid.Empty)
            throw new ArgumentException("MissionId cannot be empty.", nameof(missionId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("IdempotencyKey cannot be empty.", nameof(idempotencyKey));

        return new GlobalMissionContribution
        {
            Id = Guid.NewGuid(),
            MissionId = missionId,
            UserId = userId,
            Amount = amount,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

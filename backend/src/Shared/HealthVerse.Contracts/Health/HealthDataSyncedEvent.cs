using MediatR;

namespace HealthVerse.Contracts.Health;

/// <summary>
/// Event published when health data is successfully synchronized.
/// This allows other modules to react to health data updates.
/// For example, Competition module might update league points.
/// </summary>
public sealed record HealthDataSyncedEvent(
    Guid UserId,
    List<HealthActivityData> Activities,
    DateOnly LogDate,
    int TotalSteps,
    int PointsEarned) : INotification;

using MediatR;

namespace HealthVerse.Contracts.Gamification;

/// <summary>
/// Event published when a user earns points.
/// Used for triggering side effects like League point updates or Badge checks.
/// This is a cross-module contract event.
/// </summary>
public sealed record UserPointsEarnedEvent(
    Guid UserId, 
    long PointsEarned, 
    string SourceType, 
    DateOnly LogDate) : INotification;

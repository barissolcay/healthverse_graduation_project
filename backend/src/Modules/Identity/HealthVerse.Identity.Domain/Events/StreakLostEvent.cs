using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Identity.Domain.Events;

/// <summary>
/// Event raised when user loses their streak.
/// </summary>
public sealed record StreakLostEvent(
    Guid UserId,
    int LostStreakCount
) : DomainEventBase;

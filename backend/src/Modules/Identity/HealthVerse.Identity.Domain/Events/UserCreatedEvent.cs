using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Identity.Domain.Events;

/// <summary>
/// Event raised when a new user is created.
/// </summary>
public sealed record UserCreatedEvent(
    Guid UserId,
    string Username,
    string Email
) : DomainEventBase;

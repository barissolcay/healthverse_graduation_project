using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Identity.Domain.Events;

/// <summary>
/// Event raised when user grants health permission.
/// </summary>
public sealed record HealthPermissionGrantedEvent(
    Guid UserId,
    DateTimeOffset GrantedAt
) : DomainEventBase;

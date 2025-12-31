using MediatR;

namespace HealthVerse.SharedKernel.Domain;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain.
/// Inherits from MediatR INotification for automatic dispatching.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}

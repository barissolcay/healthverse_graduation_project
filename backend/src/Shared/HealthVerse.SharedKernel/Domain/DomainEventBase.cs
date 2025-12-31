namespace HealthVerse.SharedKernel.Domain;

/// <summary>
/// Base record for domain events.
/// Provides default implementations for EventId and OccurredAt.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

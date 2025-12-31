namespace HealthVerse.SharedKernel.Domain;

/// <summary>
/// Base class for aggregate roots.
/// An aggregate is a cluster of domain objects that can be treated as a single unit.
/// </summary>
public abstract class AggregateRoot : Entity
{
    public DateTimeOffset CreatedAt { get; protected init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    protected void SetUpdatedAt()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

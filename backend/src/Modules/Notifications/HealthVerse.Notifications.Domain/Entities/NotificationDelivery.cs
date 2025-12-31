using HealthVerse.SharedKernel.Domain;

namespace HealthVerse.Notifications.Domain.Entities;

/// <summary>
/// Notification delivery entity for the outbox pattern.
/// Tracks push notification delivery status and scheduling.
/// </summary>
public sealed class NotificationDelivery : Entity
{
    public Guid NotificationId { get; private init; }
    public Guid UserId { get; private init; }
    public DeliveryChannel Channel { get; private init; }
    public DeliveryStatus Status { get; private set; }
    public DateTimeOffset ScheduledAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private const int MaxRetries = 3;

    private NotificationDelivery() { }

    public static NotificationDelivery Create(
        Guid notificationId,
        Guid userId,
        DateTimeOffset scheduledAt,
        DeliveryChannel channel = DeliveryChannel.Push)
    {
        return new NotificationDelivery
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            UserId = userId,
            Channel = channel,
            Status = DeliveryStatus.Pending,
            ScheduledAt = scheduledAt,
            AttemptCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a delivery scheduled for later (DND quiet hours).
    /// </summary>
    public static NotificationDelivery CreateScheduled(
        Guid notificationId,
        Guid userId,
        DateTimeOffset scheduledAt)
    {
        return Create(notificationId, userId, scheduledAt);
    }

    /// <summary>
    /// Marks the delivery as successfully sent.
    /// </summary>
    public void MarkAsSent(DateTimeOffset sentAt, string? providerMessageId = null)
    {
        if (Status == DeliveryStatus.Sent)
            return;

        Status = DeliveryStatus.Sent;
        SentAt = sentAt;
        ProviderMessageId = providerMessageId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Records a failed attempt. If max retries reached, marks as failed.
    /// </summary>
    public void RecordFailedAttempt(string error, DateTimeOffset? retryAt = null)
    {
        AttemptCount++;
        LastError = error;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (AttemptCount >= MaxRetries)
        {
            Status = DeliveryStatus.Failed;
        }
        else if (retryAt.HasValue)
        {
            ScheduledAt = retryAt.Value;
        }
    }

    /// <summary>
    /// Cancels the delivery.
    /// </summary>
    public void Cancel()
    {
        if (Status == DeliveryStatus.Sent)
            throw new DomainException("NotificationDelivery.CannotCancel", "Cannot cancel a sent delivery.");

        Status = DeliveryStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Reschedules the delivery to a new time.
    /// </summary>
    public void Reschedule(DateTimeOffset newScheduledAt)
    {
        if (Status != DeliveryStatus.Pending)
            throw new DomainException("NotificationDelivery.CannotReschedule", "Can only reschedule pending deliveries.");

        ScheduledAt = newScheduledAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsReadyToSend(DateTimeOffset currentTime)
    {
        return Status == DeliveryStatus.Pending && ScheduledAt <= currentTime;
    }
}

public enum DeliveryChannel
{
    Push,
    Email,
    Sms
}

public enum DeliveryStatus
{
    Pending,
    Sent,
    Failed,
    Cancelled
}

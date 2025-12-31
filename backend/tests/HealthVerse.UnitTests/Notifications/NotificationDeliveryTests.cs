using FluentAssertions;
using HealthVerse.Notifications.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Notifications;

public class NotificationDeliveryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDelivery()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var scheduledAt = DateTimeOffset.UtcNow;

        // Act
        var delivery = NotificationDelivery.Create(notificationId, userId, scheduledAt);

        // Assert
        delivery.Id.Should().NotBe(Guid.Empty);
        delivery.NotificationId.Should().Be(notificationId);
        delivery.UserId.Should().Be(userId);
        delivery.Channel.Should().Be(DeliveryChannel.Push);
        delivery.Status.Should().Be(DeliveryStatus.Pending);
        delivery.ScheduledAt.Should().Be(scheduledAt);
        delivery.AttemptCount.Should().Be(0);
        delivery.SentAt.Should().BeNull();
        delivery.LastError.Should().BeNull();
        delivery.ProviderMessageId.Should().BeNull();
    }

    [Fact]
    public void CreateScheduled_ShouldCreateDeliveryWithScheduledTime()
    {
        // Arrange
        var scheduledAt = DateTimeOffset.UtcNow.AddHours(2);

        // Act
        var delivery = NotificationDelivery.CreateScheduled(
            Guid.NewGuid(), Guid.NewGuid(), scheduledAt);

        // Assert
        delivery.ScheduledAt.Should().Be(scheduledAt);
        delivery.Status.Should().Be(DeliveryStatus.Pending);
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateStatusAndSentAt()
    {
        // Arrange
        var delivery = NotificationDelivery.Create(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        var sentAt = DateTimeOffset.UtcNow;
        var providerMessageId = "fcm-message-123";

        // Act
        delivery.MarkAsSent(sentAt, providerMessageId);

        // Assert
        delivery.Status.Should().Be(DeliveryStatus.Sent);
        delivery.SentAt.Should().Be(sentAt);
        delivery.ProviderMessageId.Should().Be(providerMessageId);
    }

    [Fact]
    public void MarkAsSent_WhenAlreadySent_ShouldDoNothing()
    {
        // Arrange
        var delivery = NotificationDelivery.Create(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        var firstSentAt = DateTimeOffset.UtcNow;
        delivery.MarkAsSent(firstSentAt, "first-id");

        // Act
        delivery.MarkAsSent(DateTimeOffset.UtcNow.AddMinutes(1), "second-id");

        // Assert
        delivery.SentAt.Should().Be(firstSentAt);
        delivery.ProviderMessageId.Should().Be("first-id");
    }

    [Fact]
    public void RecordFailedAttempt_ShouldIncrementAttemptCount()
    {
        // Arrange
        var delivery = NotificationDelivery.Create(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        // Act
        delivery.RecordFailedAttempt("Connection timeout");

        // Assert
        delivery.AttemptCount.Should().Be(1);
        delivery.LastError.Should().Be("Connection timeout");
        delivery.Status.Should().Be(DeliveryStatus.Pending); // Still pending (max 3 retries)
    }

    [Fact]
    public void RecordFailedAttempt_WithRetryTime_ShouldUpdateScheduledAt()
    {
        // Arrange
        var delivery = NotificationDelivery.Create(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        var retryAt = DateTimeOffset.UtcNow.AddMinutes(5);

        // Act
        delivery.RecordFailedAttempt("Temporary failure", retryAt);

        // Assert
        delivery.ScheduledAt.Should().Be(retryAt);
    }

    [Fact]
    public void RecordFailedAttempt_WhenMaxRetriesReached_ShouldMarkAsFailed()
    {
        // Arrange
        var delivery = NotificationDelivery.Create(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        // Act
        delivery.RecordFailedAttempt("Error 1");
        delivery.RecordFailedAttempt("Error 2");
        delivery.RecordFailedAttempt("Error 3");

        // Assert
        delivery.AttemptCount.Should().Be(3);
        delivery.Status.Should().Be(DeliveryStatus.Failed);
    }

    [Fact]
    public void Cancel_ShouldMarkAsCancelled()
    {
        // Arrange
        var delivery = NotificationDelivery.Create(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        // Act
        delivery.Cancel();

        // Assert
        delivery.Status.Should().Be(DeliveryStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadySent_ShouldThrow()
    {
        // Arrange
        var delivery = NotificationDelivery.Create(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        delivery.MarkAsSent(DateTimeOffset.UtcNow);

        // Act
        var act = () => delivery.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot cancel a sent delivery*");
    }

    [Fact]
    public void DeliveryStatus_ShouldHaveCorrectValues()
    {
        // Assert
        DeliveryStatus.Pending.Should().Be(DeliveryStatus.Pending);
        DeliveryStatus.Sent.Should().Be(DeliveryStatus.Sent);
        DeliveryStatus.Failed.Should().Be(DeliveryStatus.Failed);
        DeliveryStatus.Cancelled.Should().Be(DeliveryStatus.Cancelled);
    }

    [Fact]
    public void DeliveryChannel_ShouldHaveCorrectValues()
    {
        // Assert
        DeliveryChannel.Push.Should().Be(DeliveryChannel.Push);
        DeliveryChannel.Email.Should().Be(DeliveryChannel.Email);
        DeliveryChannel.Sms.Should().Be(DeliveryChannel.Sms);
    }
}

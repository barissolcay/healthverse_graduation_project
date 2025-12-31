using FluentAssertions;
using HealthVerse.Notifications.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Notifications;

public class NotificationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var referenceId = Guid.NewGuid();

        // Act
        var notification = Notification.Create(
            userId, "DUEL_REQUEST", "New Duel Challenge!",
            "John challenged you to a duel", referenceId, "DUEL", "{\"challenger\": \"John\"}");

        // Assert
        notification.Id.Should().NotBe(Guid.Empty);
        notification.UserId.Should().Be(userId);
        notification.Type.Should().Be("DUEL_REQUEST");
        notification.Title.Should().Be("New Duel Challenge!");
        notification.Body.Should().Be("John challenged you to a duel");
        notification.ReferenceId.Should().Be(referenceId);
        notification.ReferenceType.Should().Be("DUEL");
        notification.Data.Should().Be("{\"challenger\": \"John\"}");
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullBody_ShouldUseEmptyString()
    {
        // Act
        var notification = Notification.Create(
            Guid.NewGuid(), "TEST", "Title", null);

        // Assert
        notification.Body.Should().Be(string.Empty);
    }

    [Fact]
    public void Create_WithCustomCreatedAt_ShouldUseProvidedTime()
    {
        // Arrange
        var customTime = DateTimeOffset.UtcNow.AddHours(-2);

        // Act
        var notification = Notification.Create(
            Guid.NewGuid(), "TEST", "Title", "Body", createdAt: customTime);

        // Assert
        notification.CreatedAt.Should().Be(customTime);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        // Act
        var act = () => Notification.Create(Guid.Empty, "TYPE", "Title", "Body");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*UserId cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyType_ShouldThrow(string? type)
    {
        // Act
        var act = () => Notification.Create(Guid.NewGuid(), type!, "Title", "Body");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Type cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldThrow(string? title)
    {
        // Act
        var act = () => Notification.Create(Guid.NewGuid(), "TYPE", title!, "Body");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Title cannot be empty*");
    }

    [Fact]
    public void MarkAsRead_ShouldSetIsReadAndReadAt()
    {
        // Arrange
        var notification = Notification.Create(Guid.NewGuid(), "TYPE", "Title", "Body");

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        notification.ReadAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkAsRead_WithCustomTime_ShouldUseProvidedTime()
    {
        // Arrange
        var notification = Notification.Create(Guid.NewGuid(), "TYPE", "Title", "Body");
        var readTime = DateTimeOffset.UtcNow.AddMinutes(-5);

        // Act
        notification.MarkAsRead(readTime);

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().Be(readTime);
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_ShouldNotChangeReadAt()
    {
        // Arrange
        var notification = Notification.Create(Guid.NewGuid(), "TYPE", "Title", "Body");
        var firstReadTime = DateTimeOffset.UtcNow.AddMinutes(-5);
        notification.MarkAsRead(firstReadTime);

        // Act
        notification.MarkAsRead(DateTimeOffset.UtcNow);

        // Assert
        notification.ReadAt.Should().Be(firstReadTime);
    }
}

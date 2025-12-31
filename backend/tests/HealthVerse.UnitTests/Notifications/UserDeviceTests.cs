using FluentAssertions;
using HealthVerse.Notifications.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Notifications;

public class UserDeviceTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDevice()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pushToken = "fcm-token-abc123";
        var platform = "ANDROID";

        // Act
        var device = UserDevice.Create(userId, pushToken, platform, "Pixel 7", "1.0.0");

        // Assert
        device.Id.Should().NotBe(Guid.Empty);
        device.UserId.Should().Be(userId);
        device.PushToken.Should().Be(pushToken);
        device.Platform.Should().Be("ANDROID");
        device.DeviceModel.Should().Be("Pixel 7");
        device.AppVersion.Should().Be("1.0.0");
        device.IsActive.Should().BeTrue();
        device.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        device.LastActiveAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldUppercasePlatform()
    {
        // Act
        var device = UserDevice.Create(Guid.NewGuid(), "token", "ios");

        // Assert
        device.Platform.Should().Be("IOS");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        // Act
        var act = () => UserDevice.Create(Guid.Empty, "token", "ANDROID");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*UserId cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyPushToken_ShouldThrow(string? pushToken)
    {
        // Act
        var act = () => UserDevice.Create(Guid.NewGuid(), pushToken!, "ANDROID");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*PushToken cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyPlatform_ShouldThrow(string? platform)
    {
        // Act
        var act = () => UserDevice.Create(Guid.NewGuid(), "token", platform!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Platform cannot be empty*");
    }

    [Fact]
    public void UpdateLastActive_ShouldUpdateLastActiveAt()
    {
        // Arrange
        var device = UserDevice.Create(Guid.NewGuid(), "token", "ANDROID");
        var originalLastActive = device.LastActiveAt;
        Thread.Sleep(10); // Small delay

        // Act
        device.UpdateLastActive();

        // Assert
        device.LastActiveAt.Should().BeAfter(originalLastActive);
    }

    [Fact]
    public void Disable_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var device = UserDevice.Create(Guid.NewGuid(), "token", "ANDROID");

        // Act
        device.Disable();

        // Assert
        device.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Enable_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var device = UserDevice.Create(Guid.NewGuid(), "token", "ANDROID");
        device.Disable();

        // Act
        device.Enable();

        // Assert
        device.IsActive.Should().BeTrue();
    }
}

using FluentAssertions;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Identity;

public class UserTests
{
    private static User CreateValidUser()
    {
        var username = Username.Create("testuser");
        var email = Email.Create("test@example.com");
        return User.Create(username, email);
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var username = Username.Create("testuser");
        var email = Email.Create("test@example.com");

        // Act
        var user = User.Create(username, email);

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.AvatarId.Should().Be(1);
        user.TotalPoints.Should().Be(0);
        user.StreakCount.Should().Be(0);
        user.FreezeInventory.Should().Be(0);
        user.CurrentTier.Should().Be("ISINMA");
        user.HealthPermissionGranted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithCustomAvatarId_ShouldCreateUserWithAvatar()
    {
        // Arrange
        var username = Username.Create("testuser");
        var email = Email.Create("test@example.com");

        // Act
        var user = User.Create(username, email, 5);

        // Assert
        user.AvatarId.Should().Be(5);
    }

    [Fact]
    public void Create_WithInvalidAvatarId_ShouldThrow()
    {
        // Arrange
        var username = Username.Create("testuser");
        var email = Email.Create("test@example.com");

        // Act
        var act = () => User.Create(username, email, 0);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*AvatarId must be positive*");
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Arrange
        var username = Username.Create("testuser");
        var email = Email.Create("test@example.com");

        // Act
        var user = User.Create(username, email);

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HealthVerse.Identity.Domain.Events.UserCreatedEvent>();
    }

    [Fact]
    public void UpdateAvatar_WithValidId_ShouldUpdate()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.UpdateAvatar(10);

        // Assert
        user.AvatarId.Should().Be(10);
    }

    [Fact]
    public void UpdateAvatar_WithInvalidId_ShouldThrow()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var act = () => user.UpdateAvatar(-1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*AvatarId must be positive*");
    }

    [Fact]
    public void UpdateBio_WithValidBio_ShouldUpdate()
    {
        // Arrange
        var user = CreateValidUser();
        var bio = "Hello, I'm a test user!";

        // Act
        user.UpdateBio(bio);

        // Assert
        user.Bio.Should().Be(bio);
    }

    [Fact]
    public void UpdateBio_WithTooLongBio_ShouldThrow()
    {
        // Arrange
        var user = CreateValidUser();
        var bio = new string('x', 151);

        // Act
        var act = () => user.UpdateBio(bio);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot exceed 150 characters*");
    }

    [Fact]
    public void UpdateCity_ShouldUpdate()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.UpdateCity("Istanbul");

        // Assert
        user.City.Should().Be("Istanbul");
    }

    [Fact]
    public void GrantHealthPermission_ShouldSetPermissionAndRaiseEvent()
    {
        // Arrange
        var user = CreateValidUser();
        user.ClearDomainEvents();
        var grantedAt = DateTimeOffset.UtcNow;

        // Act
        user.GrantHealthPermission(grantedAt);

        // Assert
        user.HealthPermissionGranted.Should().BeTrue();
        user.HealthPermissionGrantedAt.Should().Be(grantedAt);
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HealthVerse.Identity.Domain.Events.HealthPermissionGrantedEvent>();
    }

    [Fact]
    public void GrantHealthPermission_WhenAlreadyGranted_ShouldDoNothing()
    {
        // Arrange
        var user = CreateValidUser();
        var firstGrantedAt = DateTimeOffset.UtcNow;
        user.GrantHealthPermission(firstGrantedAt);
        user.ClearDomainEvents();

        // Act
        user.GrantHealthPermission(DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        user.HealthPermissionGrantedAt.Should().Be(firstGrantedAt);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RevokeHealthPermission_ShouldResetPermission()
    {
        // Arrange
        var user = CreateValidUser();
        user.GrantHealthPermission(DateTimeOffset.UtcNow);

        // Act
        user.RevokeHealthPermission();

        // Assert
        user.HealthPermissionGranted.Should().BeFalse();
        user.HealthPermissionGrantedAt.Should().BeNull();
    }

    [Fact]
    public void AddPoints_ShouldIncreaseTotal()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.AddPoints(100);

        // Assert
        user.TotalPoints.Should().Be(100);
    }

    [Fact]
    public void AddPoints_ShouldAccumulateCorrectly()
    {
        // Arrange
        var user = CreateValidUser();
        user.AddPoints(100);

        // Act
        user.AddPoints(50);

        // Assert
        user.TotalPoints.Should().Be(150);
    }

    [Fact]
    public void AddPoints_WithNegativeResultingInNegativeTotal_ShouldThrow()
    {
        // Arrange
        var user = CreateValidUser();
        user.AddPoints(100);

        // Act
        var act = () => user.AddPoints(-200);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void AddFreezeRight_ShouldIncreaseFreezeInventory()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.AddFreezeRight(3);

        // Assert
        user.FreezeInventory.Should().Be(3);
    }

    [Fact]
    public void AddFreezeRight_WithInvalidCount_ShouldThrow()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var act = () => user.AddFreezeRight(0);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*must be positive*");
    }

    [Fact]
    public void UseFreeze_WhenHasFreezes_ShouldDecrementAndReturnTrue()
    {
        // Arrange
        var user = CreateValidUser();
        user.AddFreezeRight(2);

        // Act
        var result = user.UseFreeze();

        // Assert
        result.Should().BeTrue();
        user.FreezeInventory.Should().Be(1);
    }

    [Fact]
    public void UseFreeze_WhenNoFreezes_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var result = user.UseFreeze();

        // Assert
        result.Should().BeFalse();
        user.FreezeInventory.Should().Be(0);
    }

    [Fact]
    public void UpdateStreak_ShouldUpdateStreakAndLongest()
    {
        // Arrange
        var user = CreateValidUser();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        user.UpdateStreak(5, today);

        // Assert
        user.StreakCount.Should().Be(5);
        user.LastStreakDate.Should().Be(today);
        user.LongestStreakCount.Should().Be(5);
    }

    [Fact]
    public void UpdateStreak_ShouldNotReduceLongestStreak()
    {
        // Arrange
        var user = CreateValidUser();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        user.UpdateStreak(10, today);

        // Act
        user.UpdateStreak(5, today.AddDays(1));

        // Assert
        user.StreakCount.Should().Be(5);
        user.LongestStreakCount.Should().Be(10);
    }

    [Fact]
    public void ResetStreak_WhenHasStreak_ShouldRaiseEventAndReset()
    {
        // Arrange
        var user = CreateValidUser();
        user.UpdateStreak(5, DateOnly.FromDateTime(DateTime.UtcNow));
        user.ClearDomainEvents();

        // Act
        user.ResetStreak();

        // Assert
        user.StreakCount.Should().Be(0);
        user.LastStreakDate.Should().BeNull();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HealthVerse.Identity.Domain.Events.StreakLostEvent>();
    }

    [Fact]
    public void ChangeTier_ShouldUpdateTier()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.ChangeTier("BRONZ");

        // Assert
        user.CurrentTier.Should().Be("BRONZ");
    }

    [Fact]
    public void ChangeTier_WithEmptyTier_ShouldThrow()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var act = () => user.ChangeTier("");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void IncrementCounters_ShouldIncreaseCorrectly()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.IncrementTasksCompleted();
        user.IncrementDuelsWon();
        user.IncrementGlobalMissions();
        user.IncrementFollowingCount();
        user.IncrementFollowersCount();

        // Assert
        user.TotalTasksCompleted.Should().Be(1);
        user.TotalDuelsWon.Should().Be(1);
        user.TotalGlobalMissions.Should().Be(1);
        user.FollowingCount.Should().Be(1);
        user.FollowersCount.Should().Be(1);
    }

    [Fact]
    public void DecrementFollowingCount_ShouldNotGoBelowZero()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.DecrementFollowingCount();

        // Assert
        user.FollowingCount.Should().Be(0);
    }

    [Fact]
    public void SelectTitle_ShouldUpdateSelectedTitle()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.SelectTitle("STREAK_MASTER");

        // Assert
        user.SelectedTitleId.Should().Be("STREAK_MASTER");
    }
}

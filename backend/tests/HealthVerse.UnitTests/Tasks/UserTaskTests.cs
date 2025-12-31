using FluentAssertions;
using HealthVerse.SharedKernel.Domain;
using HealthVerse.Tasks.Domain.Entities;
using Xunit;

namespace HealthVerse.UnitTests.Tasks;

public class UserTaskTests
{
    [Fact]
    public void Assign_WithValidData_ShouldCreateUserTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var validUntil = DateTimeOffset.UtcNow.AddDays(3);

        // Act
        var task = UserTask.Assign(userId, templateId, validUntil);

        // Assert
        task.Id.Should().NotBe(Guid.Empty);
        task.UserId.Should().Be(userId);
        task.TemplateId.Should().Be(templateId);
        task.CurrentValue.Should().Be(0);
        task.Status.Should().Be(UserTaskStatus.ACTIVE);
        task.ValidUntil.Should().Be(validUntil);
        task.CompletedAt.Should().BeNull();
        task.RewardClaimedAt.Should().BeNull();
        task.FailedAt.Should().BeNull();
    }

    [Fact]
    public void Assign_WithEmptyUserId_ShouldThrow()
    {
        // Act
        var act = () => UserTask.Assign(
            Guid.Empty, Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*UserId cannot be empty*");
    }

    [Fact]
    public void Assign_WithEmptyTemplateId_ShouldThrow()
    {
        // Act
        var act = () => UserTask.Assign(
            Guid.NewGuid(), Guid.Empty, DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*TemplateId cannot be empty*");
    }

    [Fact]
    public void Assign_WithPastDeadline_ShouldThrow()
    {
        // Act
        var act = () => UserTask.Assign(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(-1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*must be in the future*");
    }

    [Fact]
    public void Assign_WithDeadlineTooFar_ShouldThrow()
    {
        // Act
        var act = () => UserTask.Assign(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(8));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot exceed 7 days*");
    }

    [Fact]
    public void UpdateProgress_ShouldUpdateCurrentValue()
    {
        // Arrange
        var task = UserTask.Assign(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1));
        var targetValue = 100;

        // Act
        var completed = task.UpdateProgress(50, targetValue);

        // Assert
        completed.Should().BeFalse();
        task.CurrentValue.Should().Be(50);
        task.Status.Should().Be(UserTaskStatus.ACTIVE);
    }

    [Fact]
    public void UpdateProgress_WhenReachingTarget_ShouldComplete()
    {
        // Arrange
        var task = UserTask.Assign(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1));
        var targetValue = 100;

        // Act
        var completed = task.UpdateProgress(100, targetValue);

        // Assert
        completed.Should().BeTrue();
        task.CurrentValue.Should().Be(100);
        task.Status.Should().Be(UserTaskStatus.COMPLETED);
        task.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateProgress_ShouldCapAtTargetValue()
    {
        // Arrange
        var task = UserTask.Assign(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1));
        var targetValue = 100;

        // Act
        task.UpdateProgress(150, targetValue);

        // Assert
        task.CurrentValue.Should().Be(100);
    }

    [Fact]
    public void ClaimReward_WhenCompleted_ShouldSucceed()
    {
        // Arrange
        var task = UserTask.Assign(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1));
        task.UpdateProgress(100, 100);

        // Act
        var result = task.ClaimReward();

        // Assert
        result.Should().BeTrue();
        task.Status.Should().Be(UserTaskStatus.REWARD_CLAIMED);
        task.RewardClaimedAt.Should().NotBeNull();
    }

    [Fact]
    public void ClaimReward_WhenActive_ShouldReturnFalse()
    {
        // Arrange
        var task = UserTask.Assign(
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1));

        // Act
        var result = task.ClaimReward();

        // Assert
        result.Should().BeFalse();
        task.Status.Should().Be(UserTaskStatus.ACTIVE);
    }

    [Fact]
    public void UserTaskStatus_Constants_ShouldBeCorrect()
    {
        // Assert
        UserTaskStatus.ACTIVE.Should().Be("ACTIVE");
        UserTaskStatus.COMPLETED.Should().Be("COMPLETED");
        UserTaskStatus.REWARD_CLAIMED.Should().Be("REWARD_CLAIMED");
        UserTaskStatus.FAILED.Should().Be("FAILED");
    }
}

using FluentAssertions;
using HealthVerse.SharedKernel.Domain;
using HealthVerse.Tasks.Domain.Entities;
using Xunit;

namespace HealthVerse.UnitTests.Tasks;

public class UserGoalTests
{
    private static UserGoal CreateValidUserGoal()
    {
        return UserGoal.Create(
            userId: Guid.NewGuid(),
            title: "10K Steps Challenge",
            targetMetric: "STEPS",
            targetValue: 10000,
            validUntil: DateTimeOffset.UtcNow.AddDays(7)
        );
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateUserGoal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var validUntil = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var goal = UserGoal.Create(userId, "Run 5K", "DISTANCE", 5000, validUntil, "Run 5 kilometers", "RUNNING");

        // Assert
        goal.Id.Should().NotBe(Guid.Empty);
        goal.UserId.Should().Be(userId);
        goal.Title.Should().Be("Run 5K");
        goal.TargetMetric.Should().Be("DISTANCE");
        goal.TargetValue.Should().Be(5000);
        goal.CurrentValue.Should().Be(0);
        goal.ValidUntil.Should().Be(validUntil);
        goal.Description.Should().Be("Run 5 kilometers");
        goal.ActivityType.Should().Be("RUNNING");
        goal.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldUppercaseMetricAndActivityType()
    {
        // Act
        var goal = UserGoal.Create(
            Guid.NewGuid(), "Test", "steps", 1000, 
            DateTimeOffset.UtcNow.AddDays(1), null, "running");

        // Assert
        goal.TargetMetric.Should().Be("STEPS");
        goal.ActivityType.Should().Be("RUNNING");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        // Act
        var act = () => UserGoal.Create(
            Guid.Empty, "Test", "STEPS", 1000, DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*UserId cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldThrow(string? title)
    {
        // Act
        var act = () => UserGoal.Create(
            Guid.NewGuid(), title!, "STEPS", 1000, DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Title cannot be empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidTargetValue_ShouldThrow(int targetValue)
    {
        // Act
        var act = () => UserGoal.Create(
            Guid.NewGuid(), "Test", "STEPS", targetValue, DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*TargetValue must be positive*");
    }

    [Fact]
    public void Create_WithPastDeadline_ShouldThrow()
    {
        // Act
        var act = () => UserGoal.Create(
            Guid.NewGuid(), "Test", "STEPS", 1000, DateTimeOffset.UtcNow.AddHours(-1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*must be in the future*");
    }

    [Fact]
    public void UpdateProgress_ShouldUpdateCurrentValue()
    {
        // Arrange
        var goal = CreateValidUserGoal();

        // Act
        var completed = goal.UpdateProgress(5000);

        // Assert
        completed.Should().BeFalse();
        goal.CurrentValue.Should().Be(5000);
        goal.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateProgress_WhenReachingTarget_ShouldComplete()
    {
        // Arrange
        var goal = CreateValidUserGoal();

        // Act
        var completed = goal.UpdateProgress(10000);

        // Assert
        completed.Should().BeTrue();
        goal.CurrentValue.Should().Be(10000);
        goal.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateProgress_ShouldCapAtTargetValue()
    {
        // Arrange
        var goal = CreateValidUserGoal();

        // Act
        goal.UpdateProgress(15000);

        // Assert
        goal.CurrentValue.Should().Be(10000);
    }

    [Fact]
    public void UpdateProgress_WhenAlreadyCompleted_ShouldReturnFalse()
    {
        // Arrange
        var goal = CreateValidUserGoal();
        goal.UpdateProgress(10000);

        // Act
        var result = goal.UpdateProgress(5000);

        // Assert
        result.Should().BeFalse();
    }
}

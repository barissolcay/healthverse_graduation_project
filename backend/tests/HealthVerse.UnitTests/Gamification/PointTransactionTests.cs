using FluentAssertions;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using HealthVerse.SharedKernel.ValueObjects;
using Xunit;

namespace HealthVerse.UnitTests.Gamification;

public class PointTransactionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var idempotencyKey = IdempotencyKey.Create("TEST_KEY");

        // Act
        var transaction = PointTransaction.Create(
            userId, 100, "STEPS", idempotencyKey, 
            "source-id", "Step points", "{\"steps\": 10000}");

        // Assert
        transaction.Id.Should().NotBe(Guid.Empty);
        transaction.UserId.Should().Be(userId);
        transaction.Amount.Should().Be(100);
        transaction.SourceType.Should().Be("STEPS");
        transaction.IdempotencyKey.Should().Be(idempotencyKey);
        transaction.SourceIdText.Should().Be("source-id");
        transaction.Description.Should().Be("Step points");
        transaction.Metadata.Should().Be("{\"steps\": 10000}");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrow()
    {
        // Act
        var act = () => PointTransaction.Create(
            Guid.NewGuid(), 0, "STEPS", IdempotencyKey.Create("TEST_KEY"));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Amount cannot be zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptySourceType_ShouldThrow(string? sourceType)
    {
        // Act
        var act = () => PointTransaction.Create(
            Guid.NewGuid(), 100, sourceType!, IdempotencyKey.Create("TEST_KEY"));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*SourceType cannot be empty*");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldSucceed()
    {
        // Negative amounts are valid (for corrections/penalties)
        var act = () => PointTransaction.Create(
            Guid.NewGuid(), -50, "CORRECTION", IdempotencyKey.Create("TEST_KEY"));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void FromDailySteps_ShouldCreateCorrectTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var logDate = new DateOnly(2025, 1, 15);
        var dailySteps = 10000;
        var points = 100;

        // Act
        var transaction = PointTransaction.FromDailySteps(userId, points, logDate, dailySteps);

        // Assert
        transaction.UserId.Should().Be(userId);
        transaction.Amount.Should().Be(points);
        transaction.SourceType.Should().Be("STEPS");
        transaction.SourceIdText.Should().Be("2025-01-15");
        transaction.Description.Should().Contain("10000 steps");
        transaction.IdempotencyKey.Value.Should().Contain("STEPS_DAILY");
    }

    [Fact]
    public void FromTaskCompletion_ShouldCreateCorrectTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userTaskId = Guid.NewGuid();
        var taskTitle = "Walk 5000 steps";
        var rewardPoints = 50;

        // Act
        var transaction = PointTransaction.FromTaskCompletion(userId, rewardPoints, userTaskId, taskTitle);

        // Assert
        transaction.UserId.Should().Be(userId);
        transaction.Amount.Should().Be(rewardPoints);
        transaction.SourceType.Should().Be("TASK");
        transaction.SourceIdText.Should().Be(userTaskId.ToString());
        transaction.Description.Should().Contain(taskTitle);
        transaction.IdempotencyKey.Value.Should().Contain("TASK_REWARD");
    }
}

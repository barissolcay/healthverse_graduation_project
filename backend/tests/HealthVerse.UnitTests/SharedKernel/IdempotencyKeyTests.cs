using FluentAssertions;
using HealthVerse.SharedKernel.Domain;
using HealthVerse.SharedKernel.ValueObjects;
using Xunit;

namespace HealthVerse.UnitTests.SharedKernel;

public class IdempotencyKeyTests
{
    [Fact]
    public void Create_WithValidValue_ShouldSucceed()
    {
        // Arrange
        var value = "STEPS_DAILY:user-id:2025-01-01";

        // Act
        var key = IdempotencyKey.Create(value);

        // Assert
        key.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNull_ShouldThrow(string? value)
    {
        // Act
        var act = () => IdempotencyKey.Create(value!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrow()
    {
        // Arrange
        var value = new string('x', 201);

        // Act
        var act = () => IdempotencyKey.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot exceed 200 characters*");
    }

    [Fact]
    public void ForDailySteps_ShouldCreateCorrectKey()
    {
        // Arrange
        var userId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var logDate = new DateOnly(2025, 1, 15);

        // Act
        var key = IdempotencyKey.ForDailySteps(userId, logDate);

        // Assert
        key.Value.Should().Be("STEPS_DAILY:12345678-1234-1234-1234-123456789012:2025-01-15");
    }

    [Fact]
    public void ForWeeklyPartnerReward_ShouldCreateCorrectKey()
    {
        // Arrange
        var weekId = "2025-W03";
        var userId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var key = IdempotencyKey.ForWeeklyPartnerReward(weekId, userId);

        // Assert
        key.Value.Should().Be("WPM_REWARD:2025-W03:12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void ForGlobalMissionReward_ShouldCreateCorrectKey()
    {
        // Arrange
        var missionId = 42;
        var userId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var key = IdempotencyKey.ForGlobalMissionReward(missionId, userId);

        // Assert
        key.Value.Should().Be("GM_REWARD:42:12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void ForLeagueReward_ShouldCreateCorrectKey()
    {
        // Arrange
        var weekId = "2025-W03";
        var userId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var key = IdempotencyKey.ForLeagueReward(weekId, userId);

        // Assert
        key.Value.Should().Be("LEAGUE_REWARD:2025-W03:12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void ForMilestoneReward_ShouldCreateCorrectKey()
    {
        // Arrange
        var milestoneId = 5;
        var userId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var key = IdempotencyKey.ForMilestoneReward(milestoneId, userId);

        // Assert
        key.Value.Should().Be("MILESTONE:5:12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void ForTaskReward_ShouldCreateCorrectKey()
    {
        // Arrange
        var userTaskId = Guid.Parse("87654321-4321-4321-4321-210987654321");

        // Act
        var key = IdempotencyKey.ForTaskReward(userTaskId);

        // Assert
        key.Value.Should().Be("TASK_REWARD:87654321-4321-4321-4321-210987654321");
    }

    [Fact]
    public void ForCorrection_ShouldCreateCorrectKey()
    {
        // Arrange
        var originalTransactionId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var key = IdempotencyKey.ForCorrection(originalTransactionId);

        // Assert
        key.Value.Should().Be("CORRECTION:11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public void ForGlobalMissionContribution_ShouldCreateCorrectKey()
    {
        // Arrange
        var missionId = 10;
        var userId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var contribDate = new DateOnly(2025, 1, 15);

        // Act
        var key = IdempotencyKey.ForGlobalMissionContribution(missionId, userId, contribDate);

        // Assert
        key.Value.Should().Be("GM_CONTRIB:10:12345678-1234-1234-1234-123456789012:2025-01-15");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var key = IdempotencyKey.Create("test-key");

        // Act
        string value = key;

        // Assert
        value.Should().Be("test-key");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        var key1 = IdempotencyKey.Create("test-key");
        var key2 = IdempotencyKey.Create("test-key");

        // Assert
        key1.Equals(key2).Should().BeTrue();
    }
}

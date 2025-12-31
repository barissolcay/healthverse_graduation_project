using FluentAssertions;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Gamification;

public class MilestoneRewardTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateMilestoneReward()
    {
        // Act
        var milestone = MilestoneReward.Create(
            code: "STREAK_7",
            title: "7 Day Streak",
            description: "Maintain a 7-day streak",
            rewardType: "BADGE",
            metric: "STREAK_COUNT",
            targetValue: 7,
            freezeReward: 1,
            pointsReward: 100,
            iconName: "streak_7",
            displayOrder: 1);

        // Assert
        milestone.Id.Should().NotBe(Guid.Empty);
        milestone.Code.Should().Be("STREAK_7");
        milestone.Title.Should().Be("7 Day Streak");
        milestone.Description.Should().Be("Maintain a 7-day streak");
        milestone.RewardType.Should().Be("BADGE");
        milestone.Metric.Should().Be("STREAK_COUNT");
        milestone.TargetValue.Should().Be(7);
        milestone.FreezeReward.Should().Be(1);
        milestone.PointsReward.Should().Be(100);
        milestone.IconName.Should().Be("streak_7");
        milestone.DisplayOrder.Should().Be(1);
        milestone.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldUppercaseCodeRewardTypeAndMetric()
    {
        // Act
        var milestone = MilestoneReward.Create(
            code: "streak_7",
            title: "7 Day Streak",
            description: "Maintain a 7-day streak",
            rewardType: "badge",
            metric: "streak_count",
            targetValue: 7);

        // Assert
        milestone.Code.Should().Be("STREAK_7");
        milestone.RewardType.Should().Be("BADGE");
        milestone.Metric.Should().Be("STREAK_COUNT");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyCode_ShouldThrow(string? code)
    {
        // Act
        var act = () => MilestoneReward.Create(
            code!, "Title", "Description", "BADGE", "METRIC", 10);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Code cannot be empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidTargetValue_ShouldThrow(int targetValue)
    {
        // Act
        var act = () => MilestoneReward.Create(
            "CODE", "Title", "Description", "BADGE", "METRIC", targetValue);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*TargetValue must be positive*");
    }

    [Fact]
    public void Create_WithDefaultOptionalValues_ShouldWork()
    {
        // Act
        var milestone = MilestoneReward.Create(
            code: "TASKS_10",
            title: "Task Master",
            description: "Complete 10 tasks",
            rewardType: "TITLE",
            metric: "TASK_COUNT",
            targetValue: 10);

        // Assert
        milestone.FreezeReward.Should().Be(0);
        milestone.PointsReward.Should().Be(0);
        milestone.IconName.Should().BeNull();
        milestone.DisplayOrder.Should().Be(0);
    }
}

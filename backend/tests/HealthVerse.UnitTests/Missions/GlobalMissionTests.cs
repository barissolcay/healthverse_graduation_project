using FluentAssertions;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Missions;

public class GlobalMissionTests
{
    private static GlobalMission CreateValidMission()
    {
        return GlobalMission.Create(
            title: "World Steps Challenge",
            targetMetric: "STEPS",
            targetValue: 10_000_000,
            startDate: DateTimeOffset.UtcNow,
            endDate: DateTimeOffset.UtcNow.AddDays(7)
        );
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateMission()
    {
        // Arrange
        var startDate = DateTimeOffset.UtcNow;
        var endDate = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var mission = GlobalMission.Create(
            "World Steps Challenge", "STEPS", 10_000_000,
            startDate, endDate, "WALKING", 100);

        // Assert
        mission.Id.Should().NotBe(Guid.Empty);
        mission.Title.Should().Be("World Steps Challenge");
        mission.TargetMetric.Should().Be("STEPS");
        mission.TargetValue.Should().Be(10_000_000);
        mission.CurrentValue.Should().Be(0);
        mission.HiddenRewardPoints.Should().Be(100);
        mission.Status.Should().Be(MissionStatus.DRAFT);
        mission.StartDate.Should().Be(startDate);
        mission.EndDate.Should().Be(endDate);
        mission.ActivityType.Should().Be("WALKING");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldThrow(string? title)
    {
        // Act
        var act = () => GlobalMission.Create(
            title!, "STEPS", 10_000_000,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Title cannot be empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidTargetValue_ShouldThrow(long targetValue)
    {
        // Act
        var act = () => GlobalMission.Create(
            "Test", "STEPS", targetValue,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*TargetValue must be positive*");
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldThrow()
    {
        // Act
        var act = () => GlobalMission.Create(
            "Test", "STEPS", 10_000_000,
            DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*EndDate must be after StartDate*");
    }

    [Fact]
    public void Activate_WhenDraft_ShouldActivateMission()
    {
        // Arrange
        var mission = CreateValidMission();

        // Act
        mission.Activate();

        // Assert
        mission.Status.Should().Be(MissionStatus.ACTIVE);
    }

    [Fact]
    public void Activate_WhenNotDraft_ShouldDoNothing()
    {
        // Arrange
        var mission = CreateValidMission();
        mission.Activate();
        mission.Finish();

        // Act
        mission.Activate();

        // Assert
        mission.Status.Should().Be(MissionStatus.FINISHED);
    }

    [Fact]
    public void Finish_WhenActive_ShouldFinishMission()
    {
        // Arrange
        var mission = CreateValidMission();
        mission.Activate();

        // Act
        mission.Finish();

        // Assert
        mission.Status.Should().Be(MissionStatus.FINISHED);
    }

    [Fact]
    public void Cancel_WhenNotFinished_ShouldCancelMission()
    {
        // Arrange
        var mission = CreateValidMission();

        // Act
        mission.Cancel();

        // Assert
        mission.Status.Should().Be(MissionStatus.CANCELLED);
    }

    [Fact]
    public void Cancel_WhenFinished_ShouldDoNothing()
    {
        // Arrange
        var mission = CreateValidMission();
        mission.Activate();
        mission.Finish();

        // Act
        mission.Cancel();

        // Assert
        mission.Status.Should().Be(MissionStatus.FINISHED);
    }

    [Fact]
    public void AddContribution_WhenActive_ShouldIncreaseCurrentValue()
    {
        // Arrange
        var mission = CreateValidMission();
        mission.Activate();

        // Act
        mission.AddContribution(1000);

        // Assert
        mission.CurrentValue.Should().Be(1000);
    }

    [Fact]
    public void AddContribution_ShouldCapAtTargetValue()
    {
        // Arrange
        var mission = CreateValidMission(); // TargetValue: 10_000_000
        mission.Activate();

        // Act
        mission.AddContribution(15_000_000);

        // Assert
        mission.CurrentValue.Should().Be(10_000_000);
    }

    [Fact]
    public void AddContribution_WhenNotActive_ShouldDoNothing()
    {
        // Arrange
        var mission = CreateValidMission();

        // Act
        mission.AddContribution(1000);

        // Assert
        mission.CurrentValue.Should().Be(0);
    }

    [Fact]
    public void AddContribution_WithNegativeOrZero_ShouldDoNothing()
    {
        // Arrange
        var mission = CreateValidMission();
        mission.Activate();

        // Act
        mission.AddContribution(0);
        mission.AddContribution(-100);

        // Assert
        mission.CurrentValue.Should().Be(0);
    }

    [Fact]
    public void IsCompleted_WhenCurrentValueReachesTarget_ShouldReturnTrue()
    {
        // Arrange
        var mission = CreateValidMission();
        mission.Activate();
        mission.AddContribution(10_000_000);

        // Assert
        mission.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void IsCompleted_WhenCurrentValueBelowTarget_ShouldReturnFalse()
    {
        // Arrange
        var mission = CreateValidMission();
        mission.Activate();
        mission.AddContribution(5_000_000);

        // Assert
        mission.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenStatusIsActive_ShouldReturnTrue()
    {
        // Arrange
        var mission = CreateValidMission();
        mission.Activate();

        // Assert
        mission.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ProgressPercent_ShouldCalculateCorrectly()
    {
        // Arrange
        var mission = CreateValidMission(); // Target: 10_000_000
        mission.Activate();
        mission.AddContribution(2_500_000); // 25%

        // Assert
        mission.ProgressPercent.Should().Be(25);
    }

    [Fact]
    public void ProgressPercent_ShouldCapAt100()
    {
        // Arrange
        var mission = CreateValidMission();
        mission.Activate();
        mission.AddContribution(10_000_000);

        // Assert
        mission.ProgressPercent.Should().Be(100);
    }

    [Fact]
    public void MissionStatus_Constants_ShouldBeCorrect()
    {
        // Assert
        MissionStatus.DRAFT.Should().Be("DRAFT");
        MissionStatus.ACTIVE.Should().Be("ACTIVE");
        MissionStatus.FINISHED.Should().Be("FINISHED");
        MissionStatus.CANCELLED.Should().Be("CANCELLED");
    }
}

using FluentAssertions;
using HealthVerse.Missions.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Missions;

public class WeeklyPartnerMissionTests
{
    private static WeeklyPartnerMission CreateValidMission()
    {
        return WeeklyPartnerMission.Create(
            weekId: "2025-W01",
            initiatorId: Guid.NewGuid(),
            partnerId: Guid.NewGuid()
        );
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateMission()
    {
        // Arrange
        var weekId = "2025-W01";
        var initiatorId = Guid.NewGuid();
        var partnerId = Guid.NewGuid();

        // Act
        var mission = WeeklyPartnerMission.Create(weekId, initiatorId, partnerId);

        // Assert
        mission.Id.Should().NotBe(Guid.Empty);
        mission.WeekId.Should().Be(weekId);
        mission.InitiatorId.Should().Be(initiatorId);
        mission.PartnerId.Should().Be(partnerId);
        mission.TargetMetric.Should().Be("STEPS");
        mission.TargetValue.Should().Be(100000);
        mission.InitiatorProgress.Should().Be(0);
        mission.PartnerProgress.Should().Be(0);
        mission.Status.Should().Be(PartnerMissionStatus.ACTIVE);
    }

    [Fact]
    public void Create_WithCustomParameters_ShouldWork()
    {
        // Act
        var mission = WeeklyPartnerMission.Create(
            "2025-W01", Guid.NewGuid(), Guid.NewGuid(),
            targetMetric: "CALORIES",
            targetValue: 5000,
            activityType: "RUNNING");

        // Assert
        mission.TargetMetric.Should().Be("CALORIES");
        mission.TargetValue.Should().Be(5000);
        mission.ActivityType.Should().Be("RUNNING");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyWeekId_ShouldThrow(string? weekId)
    {
        // Act
        var act = () => WeeklyPartnerMission.Create(weekId!, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*WeekId cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyInitiatorId_ShouldThrow()
    {
        // Act
        var act = () => WeeklyPartnerMission.Create("2025-W01", Guid.Empty, Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*InitiatorId cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyPartnerId_ShouldThrow()
    {
        // Act
        var act = () => WeeklyPartnerMission.Create("2025-W01", Guid.NewGuid(), Guid.Empty);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*PartnerId cannot be empty*");
    }

    [Fact]
    public void Create_WithSameInitiatorAndPartner_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => WeeklyPartnerMission.Create("2025-W01", userId, userId);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot pair with yourself*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidTargetValue_ShouldThrow(int targetValue)
    {
        // Act
        var act = () => WeeklyPartnerMission.Create(
            "2025-W01", Guid.NewGuid(), Guid.NewGuid(), "STEPS", targetValue);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*TargetValue must be positive*");
    }

    [Fact]
    public void UpdateInitiatorProgress_WhenActive_ShouldUpdateProgress()
    {
        // Arrange
        var mission = CreateValidMission();
        var now = DateTimeOffset.UtcNow;

        // Act
        mission.UpdateInitiatorProgress(50000, now);

        // Assert
        mission.InitiatorProgress.Should().Be(50000);
        mission.UpdatedAt.Should().Be(now);
    }

    [Fact]
    public void UpdateInitiatorProgress_ShouldCapAtTargetValue()
    {
        // Arrange
        var mission = CreateValidMission();

        // Act
        mission.UpdateInitiatorProgress(150000, DateTimeOffset.UtcNow);

        // Assert
        mission.InitiatorProgress.Should().Be(100000);
    }

    [Fact]
    public void UpdatePartnerProgress_WhenActive_ShouldUpdateProgress()
    {
        // Arrange
        var mission = CreateValidMission();
        var now = DateTimeOffset.UtcNow;

        // Act
        mission.UpdatePartnerProgress(75000, now);

        // Assert
        mission.PartnerProgress.Should().Be(75000);
    }

    [Fact]
    public void Finish_WhenActive_ShouldFinishMission()
    {
        // Arrange
        var mission = CreateValidMission();
        var now = DateTimeOffset.UtcNow;

        // Act
        mission.Finish(now);

        // Assert
        mission.Status.Should().Be(PartnerMissionStatus.FINISHED);
    }

    [Fact]
    public void Cancel_WhenActive_ShouldCancelMission()
    {
        // Arrange
        var mission = CreateValidMission();
        var now = DateTimeOffset.UtcNow;

        // Act
        mission.Cancel(now);

        // Assert
        mission.Status.Should().Be(PartnerMissionStatus.CANCELLED);
    }

    [Fact]
    public void Expire_WhenActive_ShouldExpireMission()
    {
        // Arrange
        var mission = CreateValidMission();
        var now = DateTimeOffset.UtcNow;

        // Act
        mission.Expire(now);

        // Assert
        mission.Status.Should().Be(PartnerMissionStatus.EXPIRED);
    }

    [Fact]
    public void Poke_ByInitiator_ShouldSucceedOncePerDay()
    {
        // Arrange
        var mission = CreateValidMission();
        var now = DateTimeOffset.UtcNow;

        // Act
        var result1 = mission.Poke(mission.InitiatorId, now);
        var result2 = mission.Poke(mission.InitiatorId, now);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        mission.InitiatorLastPokeAt.Should().Be(now);
    }

    [Fact]
    public void Poke_ByPartner_ShouldSucceedOncePerDay()
    {
        // Arrange
        var mission = CreateValidMission();
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = mission.Poke(mission.PartnerId, now);

        // Assert
        result.Should().BeTrue();
        mission.PartnerLastPokeAt.Should().Be(now);
    }

    [Fact]
    public void Poke_ByNonParticipant_ShouldReturnFalse()
    {
        // Arrange
        var mission = CreateValidMission();

        // Act
        var result = mission.Poke(Guid.NewGuid(), DateTimeOffset.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void PartnerMissionStatus_Constants_ShouldBeCorrect()
    {
        // Assert
        PartnerMissionStatus.ACTIVE.Should().Be("ACTIVE");
        PartnerMissionStatus.FINISHED.Should().Be("FINISHED");
        PartnerMissionStatus.CANCELLED.Should().Be("CANCELLED");
        PartnerMissionStatus.EXPIRED.Should().Be("EXPIRED");
    }
}

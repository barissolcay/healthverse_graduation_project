using FluentAssertions;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using HealthVerse.SharedKernel.ValueObjects;
using Xunit;

namespace HealthVerse.UnitTests.Competition;

public class LeagueMemberTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateMember()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var weekId = WeekId.Create("2025-W01");
        var userId = Guid.NewGuid();

        // Act
        var member = LeagueMember.Create(roomId, weekId, userId);

        // Assert
        member.Id.Should().NotBe(Guid.Empty);
        member.RoomId.Should().Be(roomId);
        member.WeekId.Should().Be(weekId);
        member.UserId.Should().Be(userId);
        member.PointsInRoom.Should().Be(0);
        member.RankSnapshot.Should().BeNull();
        member.JoinedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdatePoints_WithValidPoints_ShouldUpdatePoints()
    {
        // Arrange
        var member = LeagueMember.Create(Guid.NewGuid(), WeekId.Create("2025-W01"), Guid.NewGuid());

        // Act
        member.UpdatePoints(500);

        // Assert
        member.PointsInRoom.Should().Be(500);
    }

    [Fact]
    public void UpdatePoints_WithNegativePoints_ShouldThrow()
    {
        // Arrange
        var member = LeagueMember.Create(Guid.NewGuid(), WeekId.Create("2025-W01"), Guid.NewGuid());

        // Act
        var act = () => member.UpdatePoints(-1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Points cannot be negative*");
    }

    [Fact]
    public void UpdateRank_WithValidRank_ShouldUpdateRank()
    {
        // Arrange
        var member = LeagueMember.Create(Guid.NewGuid(), WeekId.Create("2025-W01"), Guid.NewGuid());

        // Act
        member.UpdateRank(5);

        // Assert
        member.RankSnapshot.Should().Be(5);
    }

    [Fact]
    public void UpdateRank_WithInvalidRank_ShouldThrow()
    {
        // Arrange
        var member = LeagueMember.Create(Guid.NewGuid(), WeekId.Create("2025-W01"), Guid.NewGuid());

        // Act
        var act = () => member.UpdateRank(0);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Rank must be at least 1*");
    }
}

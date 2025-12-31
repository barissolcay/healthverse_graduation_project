using FluentAssertions;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using HealthVerse.SharedKernel.ValueObjects;
using Xunit;

namespace HealthVerse.UnitTests.Competition;

public class LeagueRoomTests
{
    private static LeagueRoom CreateValidRoom()
    {
        return LeagueRoom.Create(
            weekId: WeekId.Create("2025-W01"),
            tier: "BRONZ",
            startsAt: DateTimeOffset.UtcNow,
            endsAt: DateTimeOffset.UtcNow.AddDays(7)
        );
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateRoom()
    {
        // Arrange
        var weekId = WeekId.Create("2025-W01");
        var startsAt = DateTimeOffset.UtcNow;
        var endsAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var room = LeagueRoom.Create(weekId, "BRONZ", startsAt, endsAt);

        // Assert
        room.Id.Should().NotBe(Guid.Empty);
        room.WeekId.Should().Be(weekId);
        room.Tier.Should().Be("BRONZ");
        room.StartsAt.Should().Be(startsAt);
        room.EndsAt.Should().Be(endsAt);
        room.UserCount.Should().Be(0);
        room.IsProcessed.Should().BeFalse();
        room.Members.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyTier_ShouldThrow(string? tier)
    {
        // Act
        var act = () => LeagueRoom.Create(
            WeekId.Create("2025-W01"), tier!,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Tier cannot be empty*");
    }

    [Fact]
    public void Create_WithEndsAtBeforeStartsAt_ShouldThrow()
    {
        // Act
        var act = () => LeagueRoom.Create(
            WeekId.Create("2025-W01"), "BRONZ",
            DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*EndsAt must be after StartsAt*");
    }

    [Fact]
    public void AddMember_ShouldAddMemberToRoom()
    {
        // Arrange
        var room = CreateValidRoom();
        var userId = Guid.NewGuid();

        // Act
        var result = room.AddMember(userId, 50);

        // Assert
        result.IsSuccess.Should().BeTrue();
        room.UserCount.Should().Be(1);
        room.Members.Should().ContainSingle(m => m.UserId == userId);
    }

    [Fact]
    public void AddMember_WhenRoomIsFull_ShouldReturnFailure()
    {
        // Arrange
        var room = CreateValidRoom();
        var maxSize = 2;
        
        room.AddMember(Guid.NewGuid(), maxSize);
        room.AddMember(Guid.NewGuid(), maxSize);

        // Act
        var result = room.AddMember(Guid.NewGuid(), maxSize);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("LeagueRoom.Full");
    }

    [Fact]
    public void AddMember_WhenAlreadyMember_ShouldReturnFailure()
    {
        // Arrange
        var room = CreateValidRoom();
        var userId = Guid.NewGuid();
        room.AddMember(userId, 50);

        // Act
        var result = room.AddMember(userId, 50);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("LeagueRoom.AlreadyMember");
    }

    [Fact]
    public void RemoveMember_ShouldRemoveMemberFromRoom()
    {
        // Arrange
        var room = CreateValidRoom();
        var userId = Guid.NewGuid();
        room.AddMember(userId, 50);

        // Act
        room.RemoveMember(userId);

        // Assert
        room.UserCount.Should().Be(0);
        room.Members.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMember_WhenNotMember_ShouldDoNothing()
    {
        // Arrange
        var room = CreateValidRoom();
        room.AddMember(Guid.NewGuid(), 50);

        // Act
        room.RemoveMember(Guid.NewGuid()); // Different user

        // Assert
        room.UserCount.Should().Be(1);
    }

    [Fact]
    public void TryMarkAsProcessed_WhenNotProcessed_ShouldSucceed()
    {
        // Arrange
        var room = CreateValidRoom();
        var processedAt = DateTimeOffset.UtcNow;

        // Act
        var result = room.TryMarkAsProcessed(processedAt);

        // Assert
        result.Should().BeTrue();
        room.IsProcessed.Should().BeTrue();
        room.ProcessedAt.Should().Be(processedAt);
    }

    [Fact]
    public void TryMarkAsProcessed_WhenAlreadyProcessed_ShouldReturnFalse()
    {
        // Arrange
        var room = CreateValidRoom();
        room.TryMarkAsProcessed(DateTimeOffset.UtcNow);

        // Act
        var result = room.TryMarkAsProcessed(DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        result.Should().BeFalse();
    }
}

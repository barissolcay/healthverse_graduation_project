using HealthVerse.Competition.Application.Commands;
using HealthVerse.Competition.Application.Ports;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HealthVerse.UnitTests.Competition;

public sealed class JoinLeagueCommandHandlerTests
{
    private readonly ICompetitionUserRepository _userRepo = Substitute.For<ICompetitionUserRepository>();
    private readonly ILeagueConfigRepository _configRepo = Substitute.For<ILeagueConfigRepository>();
    private readonly ILeagueRoomRepository _roomRepo = Substitute.For<ILeagueRoomRepository>();
    private readonly ILeagueMemberRepository _memberRepo = Substitute.For<ILeagueMemberRepository>();
    private readonly ICompetitionUnitOfWork _unitOfWork = Substitute.For<ICompetitionUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();

    private readonly JoinLeagueCommandHandler _sut;

    public JoinLeagueCommandHandlerTests()
    {
        _sut = new JoinLeagueCommandHandler(
            _userRepo,
            _configRepo,
            _roomRepo,
            _memberRepo,
            _unitOfWork,
            _clock,
            NullLogger<JoinLeagueCommandHandler>.Instance);

        // Default clock setup - Monday of a week
        _clock.TodayTR.Returns(new DateOnly(2025, 1, 13)); // Monday
        _clock.UtcNow.Returns(new DateTimeOffset(2025, 1, 13, 10, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new JoinLeagueCommand(userId);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Kullanıcı bulunamadı.", result.Message);
    }

    [Fact]
    public async Task Handle_WhenAlreadyInRoomThisWeek_ReturnsExistingRoom()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var command = new JoinLeagueCommand(userId);
        var user = CreateTestUser(userId);
        var weekId = WeekId.FromDate(_clock.TodayTR);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var existingMembership = LeagueMember.Create(roomId, weekId, userId);
        _memberRepo.GetMembershipByUserAndWeekAsync(userId, weekId, Arg.Any<CancellationToken>())
            .Returns(existingMembership);

        var existingRoom = LeagueRoom.Create(
            weekId, 
            "ISINMA",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7));
        // Use reflection to set the Id since it's auto-generated
        typeof(LeagueRoom).GetProperty("Id")!.SetValue(existingRoom, roomId);
        
        _roomRepo.GetByIdAsync(roomId, Arg.Any<CancellationToken>())
            .Returns(existingRoom);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Zaten bu hafta bir odadasınız.", result.Message);
        Assert.Equal(roomId, result.RoomId);
    }

    [Fact]
    public async Task Handle_WhenNoAvailableRoom_CreatesNewRoom()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new JoinLeagueCommand(userId);
        var user = CreateTestUser(userId);
        var weekId = WeekId.FromDate(_clock.TodayTR);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _memberRepo.GetMembershipByUserAndWeekAsync(userId, weekId, Arg.Any<CancellationToken>())
            .Returns((LeagueMember?)null);

        var tierConfig = LeagueConfig.Create("ISINMA", 1, 5, 10, 10, 20);
        _configRepo.GetByTierNameAsync("ISINMA", Arg.Any<CancellationToken>())
            .Returns(tierConfig);

        // No available room
        _roomRepo.FindAvailableForTierAsync(weekId, "ISINMA", 20, Arg.Any<CancellationToken>())
            .Returns((LeagueRoom?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("ISINMA", result.Tier);
        
        // Verify new room was created
        await _roomRepo.Received(1).AddAsync(
            Arg.Is<LeagueRoom>(r => r.Tier == "ISINMA" && r.WeekId == weekId),
            Arg.Any<CancellationToken>());

        // Verify member was added
        await _memberRepo.Received(1).AddAsync(
            Arg.Any<LeagueMember>(),
            Arg.Any<CancellationToken>());

        // Verify SaveChanges was called (at least twice - room creation + member addition)
        await _unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAvailableRoomExists_JoinsExistingRoom()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new JoinLeagueCommand(userId);
        var user = CreateTestUser(userId);
        var weekId = WeekId.FromDate(_clock.TodayTR);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _memberRepo.GetMembershipByUserAndWeekAsync(userId, weekId, Arg.Any<CancellationToken>())
            .Returns((LeagueMember?)null);

        var tierConfig = LeagueConfig.Create("ISINMA", 3, 8, 12, 10, 15);
        _configRepo.GetByTierNameAsync("ISINMA", Arg.Any<CancellationToken>())
            .Returns(tierConfig);

        var existingRoom = LeagueRoom.Create(
            weekId,
            "ISINMA",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7));

        _roomRepo.FindAvailableForTierAsync(weekId, "ISINMA", 15, Arg.Any<CancellationToken>())
            .Returns(existingRoom);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("ISINMA", result.Tier);

        // Should NOT create new room
        await _roomRepo.DidNotReceive().AddAsync(
            Arg.Any<LeagueRoom>(),
            Arg.Any<CancellationToken>());

        // Should increment user count
        await _roomRepo.Received(1).IncrementUserCountAsync(
            existingRoom.Id,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTierConfigNotFound_DefaultsToISINMA()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new JoinLeagueCommand(userId);
        var user = CreateTestUser(userId);
        var weekId = WeekId.FromDate(_clock.TodayTR);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _memberRepo.GetMembershipByUserAndWeekAsync(userId, weekId, Arg.Any<CancellationToken>())
            .Returns((LeagueMember?)null);

        // User tier returns null (unknown tier scenario)
        _configRepo.GetByTierNameAsync("ISINMA", Arg.Any<CancellationToken>())
            .Returns((LeagueConfig?)null);

        // ISINMA tier doesn't exist either, but handler will default to it anyway
        _roomRepo.FindAvailableForTierAsync(weekId, "ISINMA", 20, Arg.Any<CancellationToken>())
            .Returns((LeagueRoom?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        // Room should be created with ISINMA tier (default)
        await _roomRepo.Received(1).AddAsync(
            Arg.Is<LeagueRoom>(r => r.Tier == "ISINMA"),
            Arg.Any<CancellationToken>());
    }

    private static User CreateTestUser(Guid userId)
    {
        var username = Username.Create("TestUser");
        var email = Email.Create("test@example.com");
        return User.Create(username, email);
    }
}

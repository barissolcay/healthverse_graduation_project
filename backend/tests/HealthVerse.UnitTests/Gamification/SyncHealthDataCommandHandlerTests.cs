using FluentAssertions;
using HealthVerse.Contracts.Health;
using HealthVerse.Gamification.Application.Commands;
using HealthVerse.Gamification.Application.Ports;
using HealthVerse.Gamification.Domain.Entities;
using HealthVerse.Gamification.Domain.Services;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.Ports;
using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.SharedKernel.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HealthVerse.UnitTests.Gamification;

public sealed class SyncHealthDataCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPointTransactionRepository _transactionRepo = Substitute.For<IPointTransactionRepository>();
    private readonly IUserDailyStatsRepository _dailyStatsRepo = Substitute.For<IUserDailyStatsRepository>();
    private readonly IGamificationUnitOfWork _unitOfWork = Substitute.For<IGamificationUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly PointCalculationService _pointCalculationService = new();

    private readonly SyncHealthDataCommandHandler _sut;

    public SyncHealthDataCommandHandlerTests()
    {
        // Default clock setup
        _clock.NowTR.Returns(DateTimeOffset.UtcNow);
        _clock.TodayTR.Returns(new DateOnly(2025, 1, 15));

        _sut = new SyncHealthDataCommandHandler(
            _userRepo,
            _transactionRepo,
            _dailyStatsRepo,
            _unitOfWork,
            _pointCalculationService,
            _clock,
            _mediator,
            Enumerable.Empty<IHealthProgressUpdater>(),
            NullLogger<SyncHealthDataCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenActivitiesEmpty_ReturnsFailure()
    {
        // Arrange
        var command = new SyncHealthDataCommand(
            Guid.NewGuid(),
            new List<HealthActivityData>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("En az bir aktivite verisi gerekli");
    }

    [Fact]
    public async Task Handle_WhenAllActivitiesManual_ReturnsFailure()
    {
        // Arrange
        var command = new SyncHealthDataCommand(
            Guid.NewGuid(),
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 1000, RecordingMethod = RecordingMethod.MANUAL },
                new() { ActivityType = "RUNNING", TargetMetric = "DISTANCE", Value = 500, RecordingMethod = RecordingMethod.UNKNOWN }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Tüm aktiviteler reddedildi");
        result.RejectedActivities.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenInvalidMetric_ReturnsFailure()
    {
        // Arrange
        var command = new SyncHealthDataCommand(
            Guid.NewGuid(),
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "INVALID_METRIC", Value = 1000, RecordingMethod = RecordingMethod.AUTOMATIC }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Geçersiz metrik");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new SyncHealthDataCommand(
            userId,
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 5000, RecordingMethod = RecordingMethod.AUTOMATIC }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Kullanıcı bulunamadı.");
    }

    [Fact]
    public async Task Handle_WhenValidSteps_CalculatesPointsAndSaves()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser();

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((PointTransaction?)null);
        _dailyStatsRepo.GetByDateAsync(userId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((UserDailyStats?)null);

        var command = new SyncHealthDataCommand(
            userId,
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 5000, RecordingMethod = RecordingMethod.AUTOMATIC }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.TotalSteps.Should().Be(5000);
        result.StepPointsEarned.Should().BeGreaterThan(0);
        await _dailyStatsRepo.Received(1).AddAsync(Arg.Any<UserDailyStats>(), Arg.Any<CancellationToken>());
        await _transactionRepo.Received(1).AddAsync(Arg.Any<PointTransaction>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAlreadyProcessedToday_SkipsStepPoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser();
        var idempotencyKey = IdempotencyKey.ForDailySteps(userId, _clock.TodayTR);
        var existingTransaction = PointTransaction.Create(userId, 50, "STEPS", idempotencyKey);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(existingTransaction);

        var command = new SyncHealthDataCommand(
            userId,
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 8000, RecordingMethod = RecordingMethod.AUTOMATIC }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.AlreadyProcessed.Should().BeTrue();
        result.StepPointsEarned.Should().Be(0);
        await _transactionRepo.DidNotReceive().AddAsync(Arg.Any<PointTransaction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMixedRecordingMethods_OnlyProcessesValidOnes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser();

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((PointTransaction?)null);
        _dailyStatsRepo.GetByDateAsync(userId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((UserDailyStats?)null);

        var command = new SyncHealthDataCommand(
            userId,
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 3000, RecordingMethod = RecordingMethod.AUTOMATIC },
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 5000, RecordingMethod = RecordingMethod.MANUAL },  // Should be rejected
                new() { ActivityType = "RUNNING", TargetMetric = "DISTANCE", Value = 2000, RecordingMethod = RecordingMethod.ACTIVE }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.RejectedActivities.Should().Be(1); // One manual entry rejected
        result.TotalSteps.Should().Be(3000); // Only automatic steps counted
    }

    [Fact]
    public async Task Handle_WhenStepsReach3000_StreakSecured()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser();

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((PointTransaction?)null);
        _dailyStatsRepo.GetByDateAsync(userId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((UserDailyStats?)null);

        var command = new SyncHealthDataCommand(
            userId,
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 3500, RecordingMethod = RecordingMethod.AUTOMATIC }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StreakSecured.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenStepsBelow3000_StreakNotSecured()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser();

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((PointTransaction?)null);
        _dailyStatsRepo.GetByDateAsync(userId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((UserDailyStats?)null);

        var command = new SyncHealthDataCommand(
            userId,
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 2000, RecordingMethod = RecordingMethod.AUTOMATIC }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StreakSecured.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenNegativeValue_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser();

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new SyncHealthDataCommand(
            userId,
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = -100, RecordingMethod = RecordingMethod.AUTOMATIC }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("negatif");
    }

    [Fact]
    public async Task Handle_WithMultipleStepEntries_TakesMaxValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser();

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((PointTransaction?)null);
        _dailyStatsRepo.GetByDateAsync(userId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((UserDailyStats?)null);

        var command = new SyncHealthDataCommand(
            userId,
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 3000, RecordingMethod = RecordingMethod.AUTOMATIC },
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 8000, RecordingMethod = RecordingMethod.AUTOMATIC },
                new() { ActivityType = "RUNNING", TargetMetric = "STEPS", Value = 5000, RecordingMethod = RecordingMethod.AUTOMATIC }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.TotalSteps.Should().Be(8000); // Max of all step values
    }

    [Fact]
    public async Task Handle_PublishesHealthDataSyncedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser();

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((PointTransaction?)null);
        _dailyStatsRepo.GetByDateAsync(userId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((UserDailyStats?)null);

        var command = new SyncHealthDataCommand(
            userId,
            new List<HealthActivityData>
            {
                new() { ActivityType = "WALKING", TargetMetric = "STEPS", Value = 5000, RecordingMethod = RecordingMethod.AUTOMATIC }
            });

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Publish(Arg.Any<HealthDataSyncedEvent>(), Arg.Any<CancellationToken>());
    }

    private static User CreateTestUser()
    {
        var username = Username.Create("TestUser");
        var email = Email.Create("test@example.com");
        return User.Create(username, email);
    }
}

using HealthVerse.Contracts.Gamification;
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

public sealed class SyncStepsCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPointTransactionRepository _transactionRepo = Substitute.For<IPointTransactionRepository>();
    private readonly IUserDailyStatsRepository _dailyStatsRepo = Substitute.For<IUserDailyStatsRepository>();
    private readonly IGamificationUnitOfWork _unitOfWork = Substitute.For<IGamificationUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    private readonly SyncStepsCommandHandler _sut;

    public SyncStepsCommandHandlerTests()
    {
        var pointCalcService = new PointCalculationService();
        
        _sut = new SyncStepsCommandHandler(
            _userRepo,
            _transactionRepo,
            _dailyStatsRepo,
            _unitOfWork,
            pointCalcService,
            _clock,
            _mediator,
            NullLogger<SyncStepsCommandHandler>.Instance);

        // Default clock setup
        _clock.TodayTR.Returns(new DateOnly(2025, 1, 15));
        _clock.UtcNow.Returns(new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task Handle_WhenStepCountIsZero_ReturnsFailure()
    {
        // Arrange
        var command = new SyncStepsCommand(Guid.NewGuid(), 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Adım sayısı 0 veya negatif olamaz.", result.Message);
        Assert.Equal(0, result.PointsEarned);
    }

    [Fact]
    public async Task Handle_WhenStepCountIsNegative_ReturnsFailure()
    {
        // Arrange
        var command = new SyncStepsCommand(Guid.NewGuid(), -100);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("negatif", result.Message);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SyncStepsCommand(userId, 5000);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Kullanıcı bulunamadı.", result.Message);
    }

    [Fact]
    public async Task Handle_WhenAlreadyProcessedToday_ReturnsAlreadyProcessed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SyncStepsCommand(userId, 5000);
        var user = CreateTestUser(userId);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Existing transaction means idempotency hit
        var idempotencyKey = IdempotencyKey.ForDailySteps(userId, _clock.TodayTR);
        var existingTx = PointTransaction.Create(
            userId,
            100,
            "STEPS",
            idempotencyKey);

        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(existingTx);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.AlreadyProcessed);
        Assert.Equal("Bu veri zaten işlenmiş.", result.Message);
    }

    [Fact]
    public async Task Handle_WhenNewStepsSync_CalculatesPointsAndSaves()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stepCount = 10000;
        var command = new SyncStepsCommand(userId, stepCount);
        var user = CreateTestUser(userId);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((PointTransaction?)null);

        _dailyStatsRepo.GetByDateAsync(userId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((UserDailyStats?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.PointsEarned > 0); // Should earn points for 10k steps
        
        // Verify daily stats was added
        await _dailyStatsRepo.Received(1).AddAsync(
            Arg.Any<UserDailyStats>(),
            Arg.Any<CancellationToken>());

        // Verify transaction was added
        await _transactionRepo.Received(1).AddAsync(
            Arg.Any<PointTransaction>(),
            Arg.Any<CancellationToken>());

        // Verify unit of work saved
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUpdatingExistingDailyStats_UpdatesInsteadOfAdding()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SyncStepsCommand(userId, 8000);
        var user = CreateTestUser(userId);
        var existingStats = UserDailyStats.Create(user.Id, _clock.TodayTR, 5000);

        _userRepo.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _transactionRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((PointTransaction?)null);

        // Use user.Id because handler calls GetByDateAsync with user.Id
        _dailyStatsRepo.GetByDateAsync(user.Id, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(existingStats);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        
        // Should NOT add new daily stats (since one exists)
        await _dailyStatsRepo.DidNotReceive().AddAsync(
            Arg.Any<UserDailyStats>(),
            Arg.Any<CancellationToken>());

        // Stats should be updated to 8000
        Assert.Equal(8000, existingStats.DailySteps);
    }

    private static User CreateTestUser(Guid userId)
    {
        var username = Username.Create("TestUser");
        var email = Email.Create("test@example.com");
        return User.Create(username, email);
    }
}

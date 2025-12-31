using FluentAssertions;
using HealthVerse.Social.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Social;

public class DuelTests
{
    private static Duel CreateValidDuel()
    {
        return Duel.Create(
            challengerId: Guid.NewGuid(),
            opponentId: Guid.NewGuid(),
            activityType: "RUNNING",
            targetMetric: "STEPS",
            targetValue: 10000,
            durationDays: 3
        );
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateDuel()
    {
        // Arrange
        var challengerId = Guid.NewGuid();
        var opponentId = Guid.NewGuid();

        // Act
        var duel = Duel.Create(challengerId, opponentId, "RUNNING", "STEPS", 10000, 3);

        // Assert
        duel.Id.Should().NotBe(Guid.Empty);
        duel.ChallengerId.Should().Be(challengerId);
        duel.OpponentId.Should().Be(opponentId);
        duel.ActivityType.Should().Be("RUNNING");
        duel.TargetMetric.Should().Be("STEPS");
        duel.TargetValue.Should().Be(10000);
        duel.DurationDays.Should().Be(3);
        duel.Status.Should().Be(DuelStatus.WAITING);
        duel.ChallengerScore.Should().Be(0);
        duel.OpponentScore.Should().Be(0);
        duel.Result.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldUppercaseActivityTypeAndMetric()
    {
        // Act
        var duel = Duel.Create(Guid.NewGuid(), Guid.NewGuid(), "running", "steps", 10000, 3);

        // Assert
        duel.ActivityType.Should().Be("RUNNING");
        duel.TargetMetric.Should().Be("STEPS");
    }

    [Fact]
    public void Create_WithEmptyChallengerId_ShouldThrow()
    {
        // Act
        var act = () => Duel.Create(Guid.Empty, Guid.NewGuid(), "RUNNING", "STEPS", 10000, 3);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*ChallengerId cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyOpponentId_ShouldThrow()
    {
        // Act
        var act = () => Duel.Create(Guid.NewGuid(), Guid.Empty, "RUNNING", "STEPS", 10000, 3);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*OpponentId cannot be empty*");
    }

    [Fact]
    public void Create_WithSameChallegerAndOpponent_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => Duel.Create(userId, userId, "RUNNING", "STEPS", 10000, 3);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot duel yourself*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyActivityType_ShouldThrow(string? activityType)
    {
        // Act
        var act = () => Duel.Create(Guid.NewGuid(), Guid.NewGuid(), activityType!, "STEPS", 10000, 3);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*ActivityType cannot be empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidTargetValue_ShouldThrow(int targetValue)
    {
        // Act
        var act = () => Duel.Create(Guid.NewGuid(), Guid.NewGuid(), "RUNNING", "STEPS", targetValue, 3);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*TargetValue must be positive*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(-1)]
    public void Create_WithInvalidDurationDays_ShouldThrow(int durationDays)
    {
        // Act
        var act = () => Duel.Create(Guid.NewGuid(), Guid.NewGuid(), "RUNNING", "STEPS", 10000, durationDays);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*DurationDays must be between 1 and 7*");
    }

    [Fact]
    public void Accept_WhenWaiting_ShouldActivateDuel()
    {
        // Arrange
        var duel = CreateValidDuel();
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = duel.Accept(now);

        // Assert
        result.Should().BeTrue();
        duel.Status.Should().Be(DuelStatus.ACTIVE);
        duel.StartDate.Should().Be(now);
        duel.EndDate.Should().Be(now.AddDays(duel.DurationDays));
    }

    [Fact]
    public void Accept_WhenNotWaiting_ShouldReturnFalse()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);

        // Act
        var result = duel.Accept(DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Reject_WhenWaiting_ShouldRejectDuel()
    {
        // Arrange
        var duel = CreateValidDuel();
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = duel.Reject(now);

        // Assert
        result.Should().BeTrue();
        duel.Status.Should().Be(DuelStatus.REJECTED);
    }

    [Fact]
    public void Reject_WhenNotWaiting_ShouldReturnFalse()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);

        // Act
        var result = duel.Reject(DateTimeOffset.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Expire_WhenWaitingAndAfter24Hours_ShouldExpireDuel()
    {
        // Arrange
        var duel = CreateValidDuel();
        var now = duel.CreatedAt.AddHours(25);

        // Act
        var result = duel.Expire(now);

        // Assert
        result.Should().BeTrue();
        duel.Status.Should().Be(DuelStatus.EXPIRED);
    }

    [Fact]
    public void Expire_WhenWaitingButBefore24Hours_ShouldReturnFalse()
    {
        // Arrange
        var duel = CreateValidDuel();
        var now = duel.CreatedAt.AddHours(23);

        // Act
        var result = duel.Expire(now);

        // Assert
        result.Should().BeFalse();
        duel.Status.Should().Be(DuelStatus.WAITING);
    }

    [Fact]
    public void UpdateChallengerScore_WhenActive_ShouldUpdateScore()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);

        // Act
        duel.UpdateChallengerScore(5000, DateTimeOffset.UtcNow);

        // Assert
        duel.ChallengerScore.Should().Be(5000);
    }

    [Fact]
    public void UpdateChallengerScore_ShouldCapAtTargetValue()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);

        // Act
        duel.UpdateChallengerScore(15000, DateTimeOffset.UtcNow);

        // Assert
        duel.ChallengerScore.Should().Be(10000); // Capped at TargetValue
    }

    [Fact]
    public void UpdateOpponentScore_WhenActive_ShouldUpdateScore()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);

        // Act
        duel.UpdateOpponentScore(7000, DateTimeOffset.UtcNow);

        // Assert
        duel.OpponentScore.Should().Be(7000);
    }

    [Fact]
    public void Finish_WhenActive_ShouldFinishAndCalculateResult()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);
        duel.UpdateChallengerScore(10000, DateTimeOffset.UtcNow);
        duel.UpdateOpponentScore(5000, DateTimeOffset.UtcNow);

        // Act
        var result = duel.Finish(DateTimeOffset.UtcNow);

        // Assert
        result.Should().BeTrue();
        duel.Status.Should().Be(DuelStatus.FINISHED);
        duel.Result.Should().Be(DuelResult.CHALLENGER_WIN);
    }

    [Fact]
    public void Finish_WhenBothReachTarget_HigherScoreWins()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);
        duel.UpdateChallengerScore(10000, DateTimeOffset.UtcNow);
        duel.UpdateOpponentScore(10000, DateTimeOffset.UtcNow);

        // Act
        duel.Finish(DateTimeOffset.UtcNow);

        // Assert
        duel.Result.Should().Be(DuelResult.BOTH_WIN);
    }

    [Fact]
    public void Finish_WhenNobodyReachesTarget_HigherScoreWins()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);
        duel.UpdateChallengerScore(3000, DateTimeOffset.UtcNow);
        duel.UpdateOpponentScore(5000, DateTimeOffset.UtcNow);

        // Act
        duel.Finish(DateTimeOffset.UtcNow);

        // Assert
        duel.Result.Should().Be(DuelResult.OPPONENT_WIN);
    }

    [Fact]
    public void Finish_WhenEqualScoresAndNobodyReaches_BothLose()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);
        duel.UpdateChallengerScore(3000, DateTimeOffset.UtcNow);
        duel.UpdateOpponentScore(3000, DateTimeOffset.UtcNow);

        // Act
        duel.Finish(DateTimeOffset.UtcNow);

        // Assert
        duel.Result.Should().Be(DuelResult.BOTH_LOSE);
    }

    [Fact]
    public void Poke_ByChallenger_ShouldSucceedOnce()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        // Act
        var result1 = duel.Poke(duel.ChallengerId, now);
        var result2 = duel.Poke(duel.ChallengerId, now);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse(); // Already poked today
        duel.ChallengerLastPokeAt.Should().Be(now);
    }

    [Fact]
    public void Poke_ByOpponent_ShouldSucceedOnce()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = duel.Poke(duel.OpponentId, now);

        // Assert
        result.Should().BeTrue();
        duel.OpponentLastPokeAt.Should().Be(now);
    }

    [Fact]
    public void Poke_ByNonParticipant_ShouldReturnFalse()
    {
        // Arrange
        var duel = CreateValidDuel();
        duel.Accept(DateTimeOffset.UtcNow);

        // Act
        var result = duel.Poke(Guid.NewGuid(), DateTimeOffset.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenActiveAndPastEndDate_ShouldReturnTrue()
    {
        // Arrange
        var duel = CreateValidDuel();
        var now = DateTimeOffset.UtcNow;
        duel.Accept(now);

        // Act
        var isExpired = duel.IsExpired(now.AddDays(4)); // 3 day duel + 1

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsParticipant_ShouldReturnTrueForParticipants()
    {
        // Arrange
        var duel = CreateValidDuel();

        // Assert
        duel.IsParticipant(duel.ChallengerId).Should().BeTrue();
        duel.IsParticipant(duel.OpponentId).Should().BeTrue();
        duel.IsParticipant(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void DuelStatus_Constants_ShouldBeCorrect()
    {
        // Assert
        DuelStatus.WAITING.Should().Be("WAITING");
        DuelStatus.ACTIVE.Should().Be("ACTIVE");
        DuelStatus.FINISHED.Should().Be("FINISHED");
        DuelStatus.REJECTED.Should().Be("REJECTED");
        DuelStatus.EXPIRED.Should().Be("EXPIRED");
    }

    [Fact]
    public void DuelResult_Constants_ShouldBeCorrect()
    {
        // Assert
        DuelResult.CHALLENGER_WIN.Should().Be("CHALLENGER_WIN");
        DuelResult.OPPONENT_WIN.Should().Be("OPPONENT_WIN");
        DuelResult.BOTH_WIN.Should().Be("BOTH_WIN");
        DuelResult.BOTH_LOSE.Should().Be("BOTH_LOSE");
    }
}

using FluentAssertions;
using HealthVerse.Social.Domain.Entities;
using Xunit;

namespace HealthVerse.UnitTests.Social;

public class UserBlockTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUserBlock()
    {
        // Arrange
        var blockerId = Guid.NewGuid();
        var blockedId = Guid.NewGuid();

        // Act
        var block = UserBlock.Create(blockerId, blockedId);

        // Assert
        block.BlockerId.Should().Be(blockerId);
        block.BlockedId.Should().Be(blockedId);
        block.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithEmptyBlockerId_ShouldThrow()
    {
        // Act
        var act = () => UserBlock.Create(Guid.Empty, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*BlockerId cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyBlockedId_ShouldThrow()
    {
        // Act
        var act = () => UserBlock.Create(Guid.NewGuid(), Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*BlockedId cannot be empty*");
    }

    [Fact]
    public void Create_WithSameBlockerAndBlocked_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => UserBlock.Create(userId, userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot block themselves*");
    }
}

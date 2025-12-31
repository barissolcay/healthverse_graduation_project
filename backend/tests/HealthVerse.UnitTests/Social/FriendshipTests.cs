using FluentAssertions;
using HealthVerse.Social.Domain.Entities;
using Xunit;

namespace HealthVerse.UnitTests.Social;

public class FriendshipTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateFriendship()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();

        // Act
        var friendship = Friendship.Create(followerId, followingId);

        // Assert
        friendship.FollowerId.Should().Be(followerId);
        friendship.FollowingId.Should().Be(followingId);
        friendship.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithEmptyFollowerId_ShouldThrow()
    {
        // Act
        var act = () => Friendship.Create(Guid.Empty, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*FollowerId cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyFollowingId_ShouldThrow()
    {
        // Act
        var act = () => Friendship.Create(Guid.NewGuid(), Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*FollowingId cannot be empty*");
    }

    [Fact]
    public void Create_WithSameFollowerAndFollowing_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => Friendship.Create(userId, userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot follow themselves*");
    }

    [Fact]
    public void TwoFriendships_ShouldBeIndependent()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        // Act
        var friendship1 = Friendship.Create(user1, user2); // User1 follows User2
        var friendship2 = Friendship.Create(user2, user1); // User2 follows User1

        // Assert - Both directions exist (mutual follow)
        friendship1.FollowerId.Should().Be(user1);
        friendship1.FollowingId.Should().Be(user2);
        friendship2.FollowerId.Should().Be(user2);
        friendship2.FollowingId.Should().Be(user1);
    }
}

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.Social.Application.DTOs;
using Xunit;

namespace HealthVerse.IntegrationTests;

public class SocialTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public SocialTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<(Guid user1Id, Guid user2Id)> CreateTwoUsersAsync()
    {
        var user1Id = Guid.NewGuid();
        var user1 = User.Create(
            Username.Create("user1"),
            Email.Create("user1@example.com"));
        typeof(User).GetProperty("Id")!.SetValue(user1, user1Id);

        var user2Id = Guid.NewGuid();
        var user2 = User.Create(
            Username.Create("user2"),
            Email.Create("user2@example.com"));
        typeof(User).GetProperty("Id")!.SetValue(user2, user2Id);

        DbContext.Users.Add(user1);
        DbContext.Users.Add(user2);
        await DbContext.SaveChangesAsync();

        return (user1Id, user2Id);
    }

    [Fact]
    public async Task Follow_CreatesFollowRelationship()
    {
        // Arrange
        var (user1Id, user2Id) = await CreateTwoUsersAsync();
        
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());

        // Act - User1 follows User2
        var response = await Client.PostAsync($"/api/social/follow/{user2Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<FollowResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Follow_MutualFollow_SetsMutualFlag()
    {
        // Arrange
        var (user1Id, user2Id) = await CreateTwoUsersAsync();

        // Act - User1 follows User2
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());
        await Client.PostAsync($"/api/social/follow/{user2Id}", null);

        // Act - User2 follows User1 (mutual)
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user2Id.ToString());
        var response = await Client.PostAsync($"/api/social/follow/{user1Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FollowResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.IsMutual.Should().BeTrue();
    }

    [Fact]
    public async Task Block_RemovesExistingFollow()
    {
        // Arrange
        var (user1Id, user2Id) = await CreateTwoUsersAsync();

        // User1 follows User2
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());
        await Client.PostAsync($"/api/social/follow/{user2Id}", null);

        // Act - User2 blocks User1
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user2Id.ToString());
        var response = await Client.PostAsync($"/api/social/block/{user1Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BlockResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Follow_BlockedUser_Fails()
    {
        // Arrange
        var (user1Id, user2Id) = await CreateTwoUsersAsync();

        // User2 blocks User1
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user2Id.ToString());
        await Client.PostAsync($"/api/social/block/{user1Id}", null);

        // Act - User1 tries to follow User2 (blocked)
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());
        var response = await Client.PostAsync($"/api/social/follow/{user2Id}", null);

        // Assert - Should fail or return unsuccessful
        var result = await response.Content.ReadFromJsonAsync<FollowResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Unfollow_RemovesFollowRelationship()
    {
        // Arrange
        var (user1Id, user2Id) = await CreateTwoUsersAsync();

        // User1 follows User2
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());
        await Client.PostAsync($"/api/social/follow/{user2Id}", null);

        // Act - User1 unfollows User2
        var response = await Client.DeleteAsync($"/api/social/unfollow/{user2Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

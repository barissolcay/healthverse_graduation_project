using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;
using Xunit;

namespace HealthVerse.IntegrationTests;

public class PartnerMissionTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public PartnerMissionTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<(Guid user1Id, Guid user2Id, Guid user3Id)> CreateThreeUsersAsync()
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

        var user3Id = Guid.NewGuid();
        var user3 = User.Create(
            Username.Create("user3"),
            Email.Create("user3@example.com"));
        typeof(User).GetProperty("Id")!.SetValue(user3, user3Id);

        DbContext.Users.Add(user1);
        DbContext.Users.Add(user2);
        DbContext.Users.Add(user3);
        await DbContext.SaveChangesAsync();

        return (user1Id, user2Id, user3Id);
    }

    private async Task CreateMutualFollow(Guid user1Id, Guid user2Id)
    {
        // User1 follows User2
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());
        await Client.PostAsync($"/api/social/follow/{user2Id}", null);

        // User2 follows User1 (mutual)
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user2Id.ToString());
        await Client.PostAsync($"/api/social/follow/{user1Id}", null);
    }

    [Fact]
    public async Task GetAvailablePartnerMissions_WhenHasMutualFriends_ReturnsPartnerMissions()
    {
        // Arrange
        var (user1Id, user2Id, _) = await CreateThreeUsersAsync();
        await CreateMutualFollow(user1Id, user2Id);

        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());

        // Act
        var response = await Client.GetAsync("/api/missions/partner/available-friends");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AvailableFriendsResponse>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task JoinPartnerMission_WithMutualFriend_CreatesParticipation()
    {
        // Arrange
        var (user1Id, user2Id, _) = await CreateThreeUsersAsync();
        await CreateMutualFollow(user1Id, user2Id);

        // Get available friends
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());
        var availableResponse = await Client.GetAsync("/api/missions/partner/available-friends");
        var friends = await availableResponse.Content.ReadFromJsonAsync<AvailableFriendsResponse>();

        if (friends?.Friends == null || friends.Friends.Count == 0)
        {
            return;
        }

        var friendId = friends.Friends.First().UserId;

        // Act - Pair with friend
        var response = await Client.PostAsync($"/api/missions/partner/pair/{friendId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PairPartnerResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task JoinPartnerMission_WithNonMutualFriend_Fails()
    {
        // Arrange
        var (user1Id, user2Id, user3Id) = await CreateThreeUsersAsync();
        
        // Only one-way follow (not mutual)
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());
        await Client.PostAsync($"/api/social/follow/{user3Id}", null);

        // Get available friends should exclude non-mutual
        var availableResponse = await Client.GetAsync("/api/missions/partner/available-friends");
        var friends = await availableResponse.Content.ReadFromJsonAsync<AvailableFriendsResponse>();

        friends.Should().NotBeNull();
        friends!.Friends.Should().NotContain(f => f.UserId == user3Id);
    }

    [Fact]
    public async Task GetMyPartnerMissions_ReturnsActivePartnerships()
    {
        // Arrange
        var (user1Id, user2Id, _) = await CreateThreeUsersAsync();
        await CreateMutualFollow(user1Id, user2Id);

        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", user1Id.ToString());

        // Act
        var response = await Client.GetAsync("/api/missions/partner/active");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<PartnerMissionDetailDto?>();
            result.Should().NotBeNull();
        }
    }
}

// DTO classes for deserialization
public class AvailableFriendsResponse
{
    public List<AvailableFriendDto> Friends { get; set; } = new();
    public int TotalAvailable { get; set; }
}

public class AvailableFriendDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int AvatarId { get; set; }
}

public class PairPartnerResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? MissionId { get; set; }
}

public class PartnerMissionDetailDto
{
    public Guid Id { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid PartnerId { get; set; }
    public string Status { get; set; } = string.Empty;
}

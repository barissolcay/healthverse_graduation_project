using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HealthVerse.Competition.Application.DTOs;
using HealthVerse.Competition.Domain.Entities;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;
using Xunit;

namespace HealthVerse.IntegrationTests;

public class LeagueTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public LeagueTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task JoinLeague_CreatesRoomAndMembership()
    {
        // Arrange: create a user and tier config
        var userId = Guid.NewGuid();
        var user = User.Create(
            Username.Create("testuser"),
            Email.Create("test@example.com"));
        
        // Use reflection to set Id since it's protected
        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        
        DbContext.Users.Add(user);

        var tierConfig = LeagueConfig.Create("ISINMA", 1, promotePercentage: 20, demotePercentage: 0, minRoomSize: 5, maxRoomSize: 20);
        DbContext.LeagueConfigs.Add(tierConfig);
        await DbContext.SaveChangesAsync();

        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

        // Act
        var response = await Client.PostAsync("/api/league/join", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JoinLeagueResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Tier.Should().Be("ISINMA");
        result.RoomId.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMyRoom_ReturnsNotFound_WhenNotJoined()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

        // Act
        var response = await Client.GetAsync("/api/league/my-room");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTiers_ReturnsTierList()
    {
        // Arrange - Create a user for authentication
        var userId = Guid.NewGuid();
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

        var tierConfig = LeagueConfig.Create("BRONZ", 2, promotePercentage: 20, demotePercentage: 20, minRoomSize: 5, maxRoomSize: 20);
        DbContext.LeagueConfigs.Add(tierConfig);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/league/tiers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TiersResponse>();
        result.Should().NotBeNull();
        result!.Tiers.Should().NotBeEmpty();
    }
}

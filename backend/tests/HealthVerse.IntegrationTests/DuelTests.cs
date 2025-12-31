using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.Social.Application.DTOs;
using HealthVerse.Competition.Application.DTOs;
using HealthVerse.Social.Domain.Entities;
using Xunit;

namespace HealthVerse.IntegrationTests;

public class DuelTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public DuelTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<(Guid user1Id, Guid user2Id)> CreateTwoUsersAsync()
    {
        var user1Id = Guid.NewGuid();
        var user1 = User.Create(
            Username.Create("challenger"),
            Email.Create("challenger@example.com"));
        typeof(User).GetProperty("Id")!.SetValue(user1, user1Id);

        var user2Id = Guid.NewGuid();
        var user2 = User.Create(
            Username.Create("opponent"),
            Email.Create("opponent@example.com"));
        typeof(User).GetProperty("Id")!.SetValue(user2, user2Id);

        DbContext.Users.Add(user1);
        DbContext.Users.Add(user2);

        // Mutual friendship is required for duel creation
        DbContext.Friendships.Add(Friendship.Create(user1Id, user2Id));
        DbContext.Friendships.Add(Friendship.Create(user2Id, user1Id));
        await DbContext.SaveChangesAsync();

        return (user1Id, user2Id);
    }

    [Fact]
    public async Task CreateDuel_ReturnsSuccess()
    {
        // Arrange
        var (challengerId, opponentId) = await CreateTwoUsersAsync();

        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", challengerId.ToString());

        var request = new CreateDuelRequest
        {
            OpponentId = opponentId,
            ActivityType = "STEPS",
            TargetMetric = "COUNT",
            TargetValue = 1000,
            DurationDays = 3
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/duels", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CreateDuelResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.DuelId.Should().NotBeNull();

        // Verify status from detail endpoint
        var duelId = result.DuelId!.Value;
        var detail = await Client.GetFromJsonAsync<DuelDetailDto>($"/api/duels/{duelId}");
        detail.Should().NotBeNull();
        detail!.Status.Should().Be(DuelStatus.WAITING);
    }

    [Fact]
    public async Task AcceptDuel_ChangesTurnToChallenger()
    {
        // Arrange
        var (challengerId, opponentId) = await CreateTwoUsersAsync();

        // Create duel
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", challengerId.ToString());

        var createRequest = new CreateDuelRequest
        {
            OpponentId = opponentId,
            ActivityType = "STEPS",
            TargetMetric = "COUNT",
            TargetValue = 1000,
            DurationDays = 3
        };
        var createResponse = await Client.PostAsJsonAsync("/api/duels", createRequest);
        var duel = await createResponse.Content.ReadFromJsonAsync<CreateDuelResponse>();
        duel.Should().NotBeNull();
        duel!.Success.Should().BeTrue();

        // Act - Opponent accepts
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", opponentId.ToString());
        var response = await Client.PostAsync($"/api/duels/{duel!.DuelId}/accept", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DuelActionResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Verify duel detail is ACTIVE
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", challengerId.ToString());
        var detail = await Client.GetFromJsonAsync<DuelDetailDto>($"/api/duels/{duel.DuelId}");
        detail.Should().NotBeNull();
        detail!.Status.Should().Be(DuelStatus.ACTIVE);
    }

    [Fact]
    public async Task PokeOpponent_WhenItsOpponentsTurn_ReturnsSuccess()
    {
        // Arrange
        var (challengerId, opponentId) = await CreateTwoUsersAsync();

        // Create duel
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", challengerId.ToString());

        var createRequest = new CreateDuelRequest
        {
            OpponentId = opponentId,
            ActivityType = "STEPS",
            TargetMetric = "COUNT",
            TargetValue = 1000,
            DurationDays = 3
        };
        var createResponse = await Client.PostAsJsonAsync("/api/duels", createRequest);
        var duel = await createResponse.Content.ReadFromJsonAsync<CreateDuelResponse>();
        duel.Should().NotBeNull();
        duel!.Success.Should().BeTrue();

        // Accept to activate before poke
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", opponentId.ToString());
        await Client.PostAsync($"/api/duels/{duel.DuelId}/accept", null);

        // Challenger pokes opponent (pending = opponent's turn)
        var response = await Client.PostAsync($"/api/duels/{duel.DuelId}/poke", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DuelActionResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeclineDuel_ChangeStatusToDeclined()
    {
        // Arrange
        var (challengerId, opponentId) = await CreateTwoUsersAsync();

        // Create duel
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", challengerId.ToString());

        var createRequest = new CreateDuelRequest
        {
            OpponentId = opponentId,
            ActivityType = "STEPS",
            TargetMetric = "COUNT",
            TargetValue = 1000,
            DurationDays = 3
        };
        var createResponse = await Client.PostAsJsonAsync("/api/duels", createRequest);
        var duel = await createResponse.Content.ReadFromJsonAsync<CreateDuelResponse>();
        duel.Should().NotBeNull();
        duel!.Success.Should().BeTrue();

        // Act - Opponent declines
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", opponentId.ToString());
        var response = await Client.PostAsync($"/api/duels/{duel.DuelId}/reject", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DuelActionResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", challengerId.ToString());
        var detail = await Client.GetFromJsonAsync<DuelDetailDto>($"/api/duels/{duel.DuelId}");
        detail.Should().NotBeNull();
        detail!.Status.Should().Be(DuelStatus.REJECTED);
    }

    [Fact]
    public async Task GetActiveDuels_ReturnsOnlyActiveDuels()
    {
        // Arrange
        var (challengerId, opponentId) = await CreateTwoUsersAsync();

        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", challengerId.ToString());

        // Create and accept a duel
        var createRequest = new CreateDuelRequest
        {
            OpponentId = opponentId,
            ActivityType = "STEPS",
            TargetMetric = "COUNT",
            TargetValue = 1000,
            DurationDays = 3
        };
        var createResponse = await Client.PostAsJsonAsync("/api/duels", createRequest);
        var duel = await createResponse.Content.ReadFromJsonAsync<CreateDuelResponse>();
        duel.Should().NotBeNull();
        duel!.Success.Should().BeTrue();

        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", opponentId.ToString());
        await Client.PostAsync($"/api/duels/{duel.DuelId}/accept", null);

        // Act - Get active duels
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", challengerId.ToString());
        var response = await Client.GetAsync("/api/duels/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ActiveDuelsResponse>();
        result.Should().NotBeNull();
        result!.Duels.Should().ContainSingle(d => d.Id == duel.DuelId);
    }
}

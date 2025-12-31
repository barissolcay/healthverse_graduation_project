using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.Missions.Domain.Entities;
using Xunit;

namespace HealthVerse.IntegrationTests;

public class GlobalMissionTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public GlobalMissionTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<GlobalMission> CreateActiveGlobalMissionAsync(string title)
    {
        var now = DateTimeOffset.UtcNow;
        var mission = GlobalMission.Create(
            title,
            targetMetric: "STEPS",
            targetValue: 10_000,
            startDate: now.AddHours(-1),
            endDate: now.AddHours(5));

        mission.Activate();

        DbContext.GlobalMissions.Add(mission);
        await DbContext.SaveChangesAsync();

        return mission;
    }

    private async Task<Guid> CreateUserAsync(string username, string email)
    {
        var userId = Guid.NewGuid();
        var user = User.Create(
            Username.Create(username),
            Email.Create(email));
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        return userId;
    }

    [Fact]
    public async Task GetActiveGlobalMissions_ReturnsCurrentMissions()
    {
        // Arrange
        await CreateActiveGlobalMissionAsync("Morning Steps");
        var userId = await CreateUserAsync("globaluser1", "globaluser1@example.com");
        
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

        // Act
        var response = await Client.GetAsync("/api/missions/global/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ActiveGlobalMissionsResponse>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task JoinGlobalMission_CreatesParticipation()
    {
        // Arrange
        await CreateActiveGlobalMissionAsync("Evening Run");
        var userId = await CreateUserAsync("globaluser2", "globaluser2@example.com");
        
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

        // Get active missions
        var activeResponse = await Client.GetAsync("/api/missions/global/active");
        var missions = await activeResponse.Content.ReadFromJsonAsync<ActiveGlobalMissionsResponse>();

        missions.Should().NotBeNull();
        missions!.Missions.Should().NotBeEmpty();

        var missionId = missions.Missions.First().Id;

        // Act
        var response = await Client.PostAsync($"/api/missions/global/{missionId}/join", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JoinMissionResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetGlobalMissionDetails_IncludesContributorCount()
    {
        // Arrange
        await CreateActiveGlobalMissionAsync("Weekend Walk");
        var userId1 = await CreateUserAsync("contributor1", "contributor1@example.com");
        var userId2 = await CreateUserAsync("contributor2", "contributor2@example.com");

        // Get active missions
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", userId1.ToString());
        
        var activeResponse = await Client.GetAsync("/api/missions/global/active");
        var missions = await activeResponse.Content.ReadFromJsonAsync<ActiveGlobalMissionsResponse>();

        missions.Should().NotBeNull();
        missions!.Missions.Should().NotBeEmpty();

        var missionId = missions.Missions.First().Id;

        // Both users join
        await Client.PostAsync($"/api/missions/global/{missionId}/join", null);
        
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", userId2.ToString());
        await Client.PostAsync($"/api/missions/global/{missionId}/join", null);

        // Act - Get details
        var response = await Client.GetAsync($"/api/missions/global/{missionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GlobalMissionDetailDto>();
        result.Should().NotBeNull();
        result!.TotalParticipants.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetGlobalMissionDetails_IncludesHoursRemaining()
    {
        // Arrange
        await CreateActiveGlobalMissionAsync("Night Ride");
        var userId = await CreateUserAsync("timeuser", "timeuser@example.com");

        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

        // Get active missions
        var activeResponse = await Client.GetAsync("/api/missions/global/active");
        var missions = await activeResponse.Content.ReadFromJsonAsync<ActiveGlobalMissionsResponse>();

        missions.Should().NotBeNull();
        missions!.Missions.Should().NotBeEmpty();

        var missionId = missions.Missions.First().Id;

        // Act
        var response = await Client.GetAsync($"/api/missions/global/{missionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GlobalMissionDetailDto>();
        result.Should().NotBeNull();
        result!.HoursRemaining.Should().BeGreaterOrEqualTo(0);
    }

}

// DTO classes for deserialization
public class ActiveGlobalMissionsResponse
{
    public List<GlobalMissionDetailDto> Missions { get; set; } = new();
    public int TotalActive { get; set; }
}

public class GlobalMissionDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ActivityType { get; set; }
    public string TargetMetric { get; set; } = string.Empty;
    public long TargetValue { get; set; }
    public long CurrentValue { get; set; }
    public int ProgressPercent { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int HoursRemaining { get; set; }
    public bool IsParticipant { get; set; }
    public long MyContribution { get; set; }
    public bool IsRewardClaimed { get; set; }
    public int TotalParticipants { get; set; }
}

public class JoinMissionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

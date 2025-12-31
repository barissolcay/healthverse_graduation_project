using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace HealthVerse.IntegrationTests;

public class StatusTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public StatusTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_ReturnsHealthy()
    {
        var response = await Client.GetAsync("/status");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<StatusResponse>();
        payload.Should().NotBeNull();
        payload!.status.Should().Be("Healthy");
    }

    private sealed record StatusResponse(string status);
}

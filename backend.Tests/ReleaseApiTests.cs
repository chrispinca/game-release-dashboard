using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace backend.Tests;

public sealed class ReleaseApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReleaseApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"))
            .CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthyPayload()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.NotNull(payload);
        Assert.Equal("healthy", payload.Status);
        Assert.NotEqual(default, payload.Timestamp);
    }

    [Fact]
    public async Task ReleasesEndpoint_ReturnsSeededReleases()
    {
        var response = await _client.GetAsync("/api/releases");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<List<ReleaseResponse>>();

        Assert.NotNull(payload);
        Assert.True(payload.Count >= 3);
        Assert.Contains(payload, release => release.GameName == "Battlefield");
    }

    [Fact]
    public async Task UnknownRelease_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/releases/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record HealthResponse(string Status, DateTimeOffset Timestamp);

    private sealed record ReleaseResponse(Guid Id, string GameName);
}

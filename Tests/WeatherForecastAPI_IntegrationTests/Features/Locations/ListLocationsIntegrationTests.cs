using System.Net;
using System.Net.Http.Json;
using WeatherForecastAPI.Features.Locations;

namespace WeatherForecastAPI.UnitTests.Features.Locations;

public class ListLocationsIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task ListLocations_EmptyDatabase_ReturnsEmptyList()
    {
        // Act - Note: Database is seeded with 3 default locations
        var response = await _client.GetAsync("/api/locations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var locations = await response.Content.ReadFromJsonAsync<List<LocationResponse>>();
        locations.Should().NotBeNull();
        locations.Should().HaveCount(3); // 3 seeded locations
        locations!.Select(l => l.Name).Should().Contain(new[] { "Warsaw", "London", "New York" });
    }

    [Fact]
    public async Task ListLocations_WithMultipleLocations_ReturnsAll()
    {
        // Arrange - Add 3 more locations (on top of 3 seeded ones)
        await _client.PostAsJsonAsync("/api/locations",
            new AddLocationRequest(48.8566m, 2.3522m, "Paris"));
        await _client.PostAsJsonAsync("/api/locations",
            new AddLocationRequest(52.5200m, 13.4050m, "Berlin"));
        await _client.PostAsJsonAsync("/api/locations",
            new AddLocationRequest(50.8503m, 4.3517m, "Brussels"));

        // Act
        var response = await _client.GetAsync("/api/locations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var locations = await response.Content.ReadFromJsonAsync<List<LocationResponse>>();
        locations.Should().NotBeNull();
        locations.Should().HaveCount(6); // 3 seeded + 3 added
        locations!.Select(l => l.Name).Should().Contain(new[] { "Paris", "Berlin", "Brussels", "Warsaw", "London", "New York" });
    }

    [Fact]
    public async Task ListLocations_OrderedByMostRecentlyUsed()
    {
        // Arrange - Add 2 new locations (in addition to the 3 seeded ones)
        var response1 = await _client.PostAsJsonAsync("/api/locations",
            new AddLocationRequest(48.8566m, 2.3522m, "First"));

        await Task.Delay(100); // Small delay

        var response2 = await _client.PostAsJsonAsync("/api/locations",
            new AddLocationRequest(52.5200m, 13.4050m, "Second"));

        // Act
        var listResponse = await _client.GetAsync("/api/locations");
        var locations = await listResponse.Content.ReadFromJsonAsync<List<LocationResponse>>();

        // Assert
        locations.Should().NotBeNull();
        locations.Should().HaveCount(5); // 3 seeded + 2 added
        locations![0].Name.Should().Be("Second"); // Most recent first
        locations[1].Name.Should().Be("First");
    }
}
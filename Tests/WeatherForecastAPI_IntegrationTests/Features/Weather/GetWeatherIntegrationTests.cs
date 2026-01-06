using System.Net;
using System.Net.Http.Json;
using WeatherForecastAPI.Features.Locations;

namespace WeatherForecastAPI.UnitTests.Features.Weather;

public class GetWeatherIntegrationTests : IAsyncLifetime
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
    public async Task GetWeather_NonExistingLocation_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/weather/locations/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWeather_ExistingLocation_UpdatesLastUsedAt()
    {
        // Arrange - Get existing Warsaw location from seeded data
        var listResponse = await _client.GetAsync("/api/locations");
        var locations = await listResponse.Content.ReadFromJsonAsync<List<LocationResponse>>();
        var warsaw = locations!.FirstOrDefault(l => l.Name == "Warsaw");

        warsaw.Should().NotBeNull();
        var locationId = warsaw!.Id;
        var originalLastUsedAt = warsaw.LastUsedAt;

        await Task.Delay(100); // Small delay

        // Act - Get weather for the location
        var weatherResponse = await _client.GetAsync($"/api/weather/locations/{locationId}");
        weatherResponse.EnsureSuccessStatusCode();

        // Assert - Fetch locations again and verify LastUsedAt was updated
        var afterListResponse = await _client.GetAsync("/api/locations");
        var afterUpdate = await afterListResponse.Content.ReadFromJsonAsync<List<LocationResponse>>();

        var warsawAfter = afterUpdate?.FirstOrDefault(l => l.Name == "Warsaw");

        warsawAfter.Should().NotBeNull();
        warsawAfter!.LastUsedAt.Should().BeAfter(originalLastUsedAt);
    }
}
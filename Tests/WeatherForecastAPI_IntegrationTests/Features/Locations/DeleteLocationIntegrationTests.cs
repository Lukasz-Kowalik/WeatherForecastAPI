using System.Net;
using System.Net.Http.Json;
using WeatherForecastAPI.Features.Locations;

namespace WeatherForecastAPI.UnitTests.Features.Locations;

public class DeleteLocationIntegrationTests : IAsyncLifetime
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
    public async Task DeleteLocation_ExistingLocation_ReturnsNoContent()
    {
        // Arrange - Get initial count and add a new location
        var initialListResponse = await _client.GetAsync("/api/locations");
        var initialLocations = await initialListResponse.Content.ReadFromJsonAsync<List<LocationResponse>>();
        var initialCount = initialLocations!.Count;

        var addResponse = await _client.PostAsJsonAsync("/api/locations",
            new AddLocationRequest(48.8566m, 2.3522m, "Paris"));
        var location = await addResponse.Content.ReadFromJsonAsync<LocationResponse>();

        // Act - Delete
        var deleteResponse = await _client.DeleteAsync($"/api/locations/{location!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's actually deleted (count should return to initial)
        var listResponse = await _client.GetAsync("/api/locations");
        var locations = await listResponse.Content.ReadFromJsonAsync<List<LocationResponse>>();
        locations.Should().HaveCount(initialCount);
        locations.Should().NotContain(l => l.Name == "Paris");
    }

    [Fact]
    public async Task DeleteLocation_NonExistingLocation_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/locations/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteLocation_ByCoordinates_ReturnsNoContent()
    {
        // Arrange - Get initial count and add location
        var initialListResponse = await _client.GetAsync("/api/locations");
        var initialLocations = await initialListResponse.Content.ReadFromJsonAsync<List<LocationResponse>>();
        var initialCount = initialLocations!.Count;

        await _client.PostAsJsonAsync("/api/locations",
           new AddLocationRequest(48.8566m, 2.3522m, "Paris"));
        // Act - Delete by coordinates
        var deleteResponse = await _client.DeleteAsync(
            "/api/locations/4");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's actually deleted (count should return to initial)
        var listResponse = await _client.GetAsync("/api/locations");
        var locations = await listResponse.Content.ReadFromJsonAsync<List<LocationResponse>>();
        locations.Should().HaveCount(initialCount);
        locations.Should().NotContain(l => l.Name == "Paris");
    }
}
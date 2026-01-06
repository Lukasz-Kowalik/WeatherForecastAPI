using System.Net;
using System.Net.Http.Json;
using WeatherForecastAPI.Features.Locations;

namespace WeatherForecastAPI.UnitTests.Features.Locations;

public class AddLocationIntegrationTests : IAsyncLifetime
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
    public async Task AddLocation_ValidRequest_ReturnsCreatedAndStoresInDb()
    {
        // Arrange
        var request = new AddLocationRequest(48.8566m, 2.3522m, "Paris Test");

        // Act
        var response = await _client.PostAsJsonAsync("/api/locations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<LocationResponse>();
        content.Should().NotBeNull();
        content!.Name.Should().Be("Paris Test");
        content.Latitude.Should().Be(48.8566m);
        content.Longitude.Should().Be(2.3522m);
        content.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AddLocation_InvalidLatitude_ReturnsBadRequest()
    {
        // Arrange
        var request = new AddLocationRequest(150m, 21.0122m, "Invalid");

        // Act
        var response = await _client.PostAsJsonAsync("/api/locations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddLocation_InvalidLongitude_ReturnsBadRequest()
    {
        // Arrange
        var request = new AddLocationRequest(52.2297m, 200m, "Invalid");

        // Act
        var response = await _client.PostAsJsonAsync("/api/locations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddLocation_DuplicateCoordinates_ReturnsExisting()
    {
        // Arrange - Use coordinates that don't exist in seeded data
        var request = new AddLocationRequest(35.6762m, 139.6503m, "Tokyo");

        // Act - Add first time
        var response1 = await _client.PostAsJsonAsync("/api/locations", request);
        var location1 = await response1.Content.ReadFromJsonAsync<LocationResponse>();

        // Act - Add second time (same coordinates)
        var response2 = await _client.PostAsJsonAsync("/api/locations", request);
        var location2 = await response2.Content.ReadFromJsonAsync<LocationResponse>();

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        location1!.Id.Should().Be(location2!.Id); // Same location returned
    }

    [Fact]
    public async Task AddLocation_WithoutName_ReturnsCreated()
    {
        // Arrange - Use unique coordinates
        var request = new AddLocationRequest(34.0522m, -118.2437m);

        // Act
        var response = await _client.PostAsJsonAsync("/api/locations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<LocationResponse>();
        content.Should().NotBeNull();
        content!.Name.Should().BeNull();
    }

    [Fact]
    public async Task AddLocation_WithLongName_ReturnsBadRequest()
    {
        // Arrange
        var longName = new string('A', 201); // 201 characters (max is 200)
        var request = new AddLocationRequest(37.7749m, -122.4194m, longName);

        // Act
        var response = await _client.PostAsJsonAsync("/api/locations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
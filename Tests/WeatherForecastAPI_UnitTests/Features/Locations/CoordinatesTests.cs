using FluentAssertions;
using WeatherForecastAPI.Features.Locations;
using Xunit;

namespace WeatherForecastAPI_UnitTests.Features.Locations;

public class CoordinatesTests
{
    [Fact]
    public void Create_ValidCoordinates_ReturnsCoordinates()
    {
        // Arrange
        decimal latitude = 40.7128m;
        decimal longitude = -74.0060m;

        // Act
        var coordinates = Coordinates.Create(latitude, longitude);

        // Assert
        coordinates.Should().NotBeNull();
        coordinates.Latitude.Should().Be(latitude);
        coordinates.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Create_LatitudeNegative90_ReturnsCoordinates()
    {
        // Arrange
        decimal latitude = -90m;
        decimal longitude = 0m;

        // Act
        var coordinates = Coordinates.Create(latitude, longitude);

        // Assert
        coordinates.Latitude.Should().Be(latitude);
        coordinates.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Create_LatitudePositive90_ReturnsCoordinates()
    {
        // Arrange
        decimal latitude = 90m;
        decimal longitude = 0m;

        // Act
        var coordinates = Coordinates.Create(latitude, longitude);

        // Assert
        coordinates.Latitude.Should().Be(latitude);
        coordinates.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Create_LongitudeNegative180_ReturnsCoordinates()
    {
        // Arrange
        decimal latitude = 0m;
        decimal longitude = -180m;

        // Act
        var coordinates = Coordinates.Create(latitude, longitude);

        // Assert
        coordinates.Latitude.Should().Be(latitude);
        coordinates.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Create_LongitudePositive180_ReturnsCoordinates()
    {
        // Arrange
        decimal latitude = 0m;
        decimal longitude = 180m;

        // Act
        var coordinates = Coordinates.Create(latitude, longitude);

        // Assert
        coordinates.Latitude.Should().Be(latitude);
        coordinates.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Create_LatitudeGreaterThan90_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        decimal latitude = 90.1m;
        decimal longitude = 0m;

        // Act & Assert
        var action = () => Coordinates.Create(latitude, longitude);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Fact]
    public void Create_LatitudeLessThanNegative90_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        decimal latitude = -90.1m;
        decimal longitude = 0m;

        // Act & Assert
        var action = () => Coordinates.Create(latitude, longitude);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Fact]
    public void Create_LongitudeGreaterThan180_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        decimal latitude = 0m;
        decimal longitude = 180.1m;

        // Act & Assert
        var action = () => Coordinates.Create(latitude, longitude);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }

    [Fact]
    public void Create_LongitudeLessThanNegative180_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        decimal latitude = 0m;
        decimal longitude = -180.1m;

        // Act & Assert
        var action = () => Coordinates.Create(latitude, longitude);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }

    [Fact]
    public void Create_BothOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert - should fail on latitude check first
        var action = () => Coordinates.Create(100m, 200m);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Fact]
    public void Create_EquatorAndPrimeMeridian_ReturnsCoordinates()
    {
        // Arrange
        decimal latitude = 0m;
        decimal longitude = 0m;

        // Act
        var coordinates = Coordinates.Create(latitude, longitude);

        // Assert
        coordinates.Latitude.Should().Be(latitude);
        coordinates.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(45.5, 120.3)]
    [InlineData(-33.8, 151.2)]
    [InlineData(35.6, 139.7)]
    public void Create_VariousValidCoordinates_ReturnsCoordinates(double latitude, double longitude)
    {
        // Act
        var coordinates = Coordinates.Create((decimal)latitude, (decimal)longitude);

        // Assert
        coordinates.Latitude.Should().Be((decimal)latitude);
        coordinates.Longitude.Should().Be((decimal)longitude);
    }

    [Fact]
    public void Coordinates_IsRecord_SupportsEquality()
    {
        // Arrange
        var coord1 = Coordinates.Create(40.7128m, -74.0060m);
        var coord2 = Coordinates.Create(40.7128m, -74.0060m);

        // Act & Assert
        coord1.Should().Be(coord2);
    }

    [Fact]
    public void Coordinates_DifferentValues_NotEqual()
    {
        // Arrange
        var coord1 = Coordinates.Create(40.7128m, -74.0060m);
        var coord2 = Coordinates.Create(51.5074m, -0.1278m);

        // Act & Assert
        coord1.Should().NotBe(coord2);
    }
}

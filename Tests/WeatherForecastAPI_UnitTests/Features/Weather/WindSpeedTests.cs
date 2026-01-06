using FluentAssertions;
using WeatherForecastAPI.Features.Weather;
using Xunit;

namespace WeatherForecastAPI_UnitTests.Features.Weather;

public class WindSpeedTests
{
    [Fact]
    public void Create_WithPositiveWindSpeed_ReturnsWindSpeed()
    {
        // Arrange
        decimal value = 10.5m;

        // Act
        var windSpeed = WindSpeed.Create(value);

        // Assert
        windSpeed.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithZeroWindSpeed_ReturnsWindSpeed()
    {
        // Arrange
        decimal value = 0m;

        // Act
        var windSpeed = WindSpeed.Create(value);

        // Assert
        windSpeed.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithHighWindSpeed_ReturnsWindSpeed()
    {
        // Arrange
        decimal value = 100m;

        // Act
        var windSpeed = WindSpeed.Create(value);

        // Assert
        windSpeed.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithDecimalPrecision_ReturnsWindSpeed()
    {
        // Arrange
        decimal value = 15.7m;

        // Act
        var windSpeed = WindSpeed.Create(value);

        // Assert
        windSpeed.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithNegativeWindSpeed_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        decimal value = -1m;

        // Act & Assert
        var action = () => WindSpeed.Create(value);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value")
            .WithMessage("*Wind speed cannot be negative*");
    }

    [Fact]
    public void Create_WithSmallNegativeWindSpeed_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        decimal value = -0.1m;

        // Act & Assert
        var action = () => WindSpeed.Create(value);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Create_WithLargeNegativeWindSpeed_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        decimal value = -100m;

        // Act & Assert
        var action = () => WindSpeed.Create(value);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10.5)]
    [InlineData(25.75)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Create_WithVariousValidWindSpeeds_ReturnsWindSpeed(double value)
    {
        // Act
        var windSpeed = WindSpeed.Create((decimal)value);

        // Assert
        windSpeed.Value.Should().Be((decimal)value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.5)]
    [InlineData(-10)]
    [InlineData(-100.5)]
    public void Create_WithVariousNegativeWindSpeeds_ThrowsException(double value)
    {
        // Act & Assert
        var action = () => WindSpeed.Create((decimal)value);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WindSpeed_IsRecord_SupportsEquality()
    {
        // Arrange
        var windSpeed1 = WindSpeed.Create(10.5m);
        var windSpeed2 = WindSpeed.Create(10.5m);

        // Act & Assert
        windSpeed1.Should().Be(windSpeed2);
    }

    [Fact]
    public void WindSpeed_DifferentValues_NotEqual()
    {
        // Arrange
        var windSpeed1 = WindSpeed.Create(10.5m);
        var windSpeed2 = WindSpeed.Create(15.0m);

        // Act & Assert
        windSpeed1.Should().NotBe(windSpeed2);
    }

    [Fact]
    public void WindSpeed_CanBeUsedInCollection()
    {
        // Arrange
        var windSpeeds = new List<WindSpeed>
        {
            WindSpeed.Create(5m),
            WindSpeed.Create(10.5m),
            WindSpeed.Create(15m)
        };

        // Assert
        windSpeeds.Should().HaveCount(3);
        windSpeeds.Should().Contain(WindSpeed.Create(10.5m));
    }

    [Fact]
    public void WindSpeed_HasReadOnlyValue()
    {
        // Arrange
        var windSpeed = WindSpeed.Create(10m);

        // Assert
        windSpeed.Value.Should().Be(10m);
    }
}

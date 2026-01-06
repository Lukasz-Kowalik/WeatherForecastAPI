using FluentAssertions;
using WeatherForecastAPI.Features.Weather;
using Xunit;

namespace WeatherForecastAPI_UnitTests.Features.Weather;

public class TemperatureTests
{
    [Fact]
    public void Create_WithValidTemperatures_ReturnsTemperature()
    {
        // Arrange
        decimal current = 20m;
        decimal maximum = 25m;
        decimal minimum = 15m;

        // Act
        var temperature = Temperature.Create(current, maximum, minimum);

        // Assert
        temperature.Current.Should().Be(current);
        temperature.Maximum.Should().Be(maximum);
        temperature.Minimum.Should().Be(minimum);
    }

    [Fact]
    public void Create_WithEqualMaxAndMin_ReturnsTemperature()
    {
        // Arrange
        decimal current = 20m;
        decimal maximum = 20m;
        decimal minimum = 20m;

        // Act
        var temperature = Temperature.Create(current, maximum, minimum);

        // Assert
        temperature.Current.Should().Be(current);
        temperature.Maximum.Should().Be(maximum);
        temperature.Minimum.Should().Be(minimum);
    }

    [Fact]
    public void Create_WithMaxEqualToMin_ReturnsTemperature()
    {
        // Arrange
        decimal current = 15m;
        decimal maximum = 20m;
        decimal minimum = 20m;

        // Act
        var temperature = Temperature.Create(current, maximum, minimum);

        // Assert
        temperature.Current.Should().Be(current);
        temperature.Maximum.Should().Be(maximum);
        temperature.Minimum.Should().Be(minimum);
    }

    [Fact]
    public void Create_WithMaxLessThanMin_ThrowsArgumentException()
    {
        // Arrange
        decimal current = 20m;
        decimal maximum = 15m;
        decimal minimum = 25m;

        // Act & Assert
        var action = () => Temperature.Create(current, maximum, minimum);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("maximum")
            .WithMessage("*Maximum temperature*cannot be lower than minimum*");
    }

    [Fact]
    public void Create_WithNegativeTemperatures_ReturnsTemperature()
    {
        // Arrange
        decimal current = -10m;
        decimal maximum = -5m;
        decimal minimum = -20m;

        // Act
        var temperature = Temperature.Create(current, maximum, minimum);

        // Assert
        temperature.Current.Should().Be(current);
        temperature.Maximum.Should().Be(maximum);
        temperature.Minimum.Should().Be(minimum);
    }

    [Fact]
    public void Create_WithZeroTemperatures_ReturnsTemperature()
    {
        // Arrange
        decimal current = 0m;
        decimal maximum = 10m;
        decimal minimum = -10m;

        // Act
        var temperature = Temperature.Create(current, maximum, minimum);

        // Assert
        temperature.Current.Should().Be(current);
        temperature.Maximum.Should().Be(maximum);
        temperature.Minimum.Should().Be(minimum);
    }

    [Fact]
    public void Create_WithDecimalPrecision_ReturnsTemperature()
    {
        // Arrange
        decimal current = 20.5m;
        decimal maximum = 25.7m;
        decimal minimum = 15.3m;

        // Act
        var temperature = Temperature.Create(current, maximum, minimum);

        // Assert
        temperature.Current.Should().Be(current);
        temperature.Maximum.Should().Be(maximum);
        temperature.Minimum.Should().Be(minimum);
    }

    [Fact]
    public void Create_WithCurrentAboveMax_ReturnsTemperature()
    {
        // Arrange
        decimal current = 30m;
        decimal maximum = 25m;
        decimal minimum = 15m;

        // Act
        var temperature = Temperature.Create(current, maximum, minimum);

        // Assert
        temperature.Current.Should().Be(current);
        temperature.Current.Should().BeGreaterThan(temperature.Maximum);
    }

    [Fact]
    public void Create_WithCurrentBelowMin_ReturnsTemperature()
    {
        // Arrange
        decimal current = 10m;
        decimal maximum = 25m;
        decimal minimum = 15m;

        // Act
        var temperature = Temperature.Create(current, maximum, minimum);

        // Assert
        temperature.Current.Should().Be(current);
        temperature.Current.Should().BeLessThan(temperature.Minimum);
    }

    [Theory]
    [InlineData(20, 25, 15)]
    [InlineData(0, 10, -10)]
    [InlineData(-5, 5, -15)]
    [InlineData(100, 100, 100)]
    public void Create_WithVariousValidTemperatures_ReturnsTemperature(decimal current, decimal maximum, decimal minimum)
    {
        // Act
        var temperature = Temperature.Create(current, maximum, minimum);

        // Assert
        temperature.Current.Should().Be(current);
        temperature.Maximum.Should().Be(maximum);
        temperature.Minimum.Should().Be(minimum);
    }

    [Theory]
    [InlineData(20, 10, 15)]
    [InlineData(0, -5, 5)]
    [InlineData(-10, -20, -5)]
    public void Create_WithMaxLessThanMin_ThrowsException(decimal current, decimal maximum, decimal minimum)
    {
        // Act & Assert
        var action = () => Temperature.Create(current, maximum, minimum);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Temperature_IsRecord_SupportsEquality()
    {
        // Arrange
        var temp1 = Temperature.Create(20m, 25m, 15m);
        var temp2 = Temperature.Create(20m, 25m, 15m);

        // Act & Assert
        temp1.Should().Be(temp2);
    }

    [Fact]
    public void Temperature_DifferentValues_NotEqual()
    {
        // Arrange
        var temp1 = Temperature.Create(20m, 25m, 15m);
        var temp2 = Temperature.Create(21m, 25m, 15m);

        // Act & Assert
        temp1.Should().NotBe(temp2);
    }

    [Fact]
    public void Temperature_CanBeUsedInCollection()
    {
        // Arrange
        var temps = new List<Temperature>
        {
            Temperature.Create(20m, 25m, 15m),
            Temperature.Create(21m, 26m, 16m),
            Temperature.Create(19m, 24m, 14m)
        };

        // Assert
        temps.Should().HaveCount(3);
        temps.Should().Contain(Temperature.Create(20m, 25m, 15m));
    }
}

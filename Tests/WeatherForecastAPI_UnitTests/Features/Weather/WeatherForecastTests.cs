using FluentAssertions;
using WeatherForecastAPI.Features.Locations;
using WeatherForecastAPI.Features.Weather;
using Xunit;

namespace WeatherForecastAPI_UnitTests.Features.Weather;

public class WeatherForecastTests
{
    private readonly int _validLocationId = 1;
    private readonly DateOnly _validForecastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private readonly Temperature _validTemperature = Temperature.Create(20m, 25m, 15m);
    private readonly WindSpeed _validWindSpeed = WindSpeed.Create(10m);
    private readonly int _validWeatherCode = 1;

    [Fact]
    public void Create_WithValidParameters_ReturnsWeatherForecast()
    {
        // Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);

        // Assert
        forecast.Should().NotBeNull();
        forecast.LocationId.Should().Be(_validLocationId);
        forecast.ForecastDate.Should().Be(_validForecastDate);
        forecast.Temperature.Should().Be(_validTemperature);
        forecast.WindSpeed.Should().Be(_validWindSpeed);
        forecast.WeatherCode.Should().Be(_validWeatherCode);
    }

    [Fact]
    public void Create_SetsRetrievedAtToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);
        var afterCreation = DateTime.UtcNow;

        // Assert
        forecast.RetrievedAt.Should().BeOnOrAfter(beforeCreation);
        forecast.RetrievedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Create_WithZeroLocationId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int invalidLocationId = 0;

        // Act & Assert
        var action = () => WeatherForecast.Create(
            invalidLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("locationId");
    }

    [Fact]
    public void Create_WithNegativeLocationId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int invalidLocationId = -1;

        // Act & Assert
        var action = () => WeatherForecast.Create(
            invalidLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("locationId");
    }

    [Fact]
    public void Create_WithNegativeWeatherCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int invalidWeatherCode = -1;

        // Act & Assert
        var action = () => WeatherForecast.Create(
            _validLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            invalidWeatherCode);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("weatherCode");
    }

    [Fact]
    public void Create_WithZeroWeatherCode_ReturnsWeatherForecast()
    {
        // Arrange
        int weatherCode = 0;

        // Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            weatherCode);

        // Assert
        forecast.WeatherCode.Should().Be(weatherCode);
    }

    [Fact]
    public void Create_WithHighWeatherCode_ReturnsWeatherForecast()
    {
        // Arrange
        int weatherCode = 99;

        // Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            weatherCode);

        // Assert
        forecast.WeatherCode.Should().Be(weatherCode);
    }

    [Fact]
    public void Create_WithFutureDate_ReturnsWeatherForecast()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        // Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            futureDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);

        // Assert
        forecast.ForecastDate.Should().Be(futureDate);
    }

    [Fact]
    public void Create_WithPastDate_ReturnsWeatherForecast()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        // Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            pastDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);

        // Assert
        forecast.ForecastDate.Should().Be(pastDate);
    }

    [Fact]
    public void Create_WithTodayDate_ReturnsWeatherForecast()
    {
        // Arrange
        var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            todayDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);

        // Assert
        forecast.ForecastDate.Should().Be(todayDate);
    }

    [Fact]
    public void Create_WithVariousTemperatures_ReturnsWeatherForecast()
    {
        // Arrange
        var temperature1 = Temperature.Create(0m, 5m, -5m);
        var temperature2 = Temperature.Create(35m, 40m, 30m);

        // Act
        var forecast1 = WeatherForecast.Create(_validLocationId, _validForecastDate, temperature1, _validWindSpeed, _validWeatherCode);
        var forecast2 = WeatherForecast.Create(_validLocationId, _validForecastDate, temperature2, _validWindSpeed, _validWeatherCode);

        // Assert
        forecast1.Temperature.Should().Be(temperature1);
        forecast2.Temperature.Should().Be(temperature2);
    }

    [Fact]
    public void Create_WithVariousWindSpeeds_ReturnsWeatherForecast()
    {
        // Arrange
        var windSpeed1 = WindSpeed.Create(0m);
        var windSpeed2 = WindSpeed.Create(50.5m);

        // Act
        var forecast1 = WeatherForecast.Create(_validLocationId, _validForecastDate, _validTemperature, windSpeed1, _validWeatherCode);
        var forecast2 = WeatherForecast.Create(_validLocationId, _validForecastDate, _validTemperature, windSpeed2, _validWeatherCode);

        // Assert
        forecast1.WindSpeed.Should().Be(windSpeed1);
        forecast2.WindSpeed.Should().Be(windSpeed2);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Create_WithVariousValidLocationIds_ReturnsWeatherForecast(int locationId)
    {
        // Act
        var forecast = WeatherForecast.Create(
            locationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);

        // Assert
        forecast.LocationId.Should().Be(locationId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidLocationIds_ThrowsException(int locationId)
    {
        // Act & Assert
        var action = () => WeatherForecast.Create(
            locationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Create_WithInvalidWeatherCodes_ThrowsException(int weatherCode)
    {
        // Act & Assert
        var action = () => WeatherForecast.Create(
            _validLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            weatherCode);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_LocationPropertyIsInitiallyNull()
    {
        // Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);

        // Assert
        forecast.Location.Should().BeNull();
    }

    [Fact]
    public void Create_MultipleForecasts_HaveIndependentState()
    {
        // Arrange
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var date2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var temp1 = Temperature.Create(20m, 25m, 15m);
        var temp2 = Temperature.Create(25m, 30m, 20m);

        // Act
        var forecast1 = WeatherForecast.Create(1, date1, temp1, _validWindSpeed, 1);
        var forecast2 = WeatherForecast.Create(2, date2, temp2, _validWindSpeed, 2);

        // Assert
        forecast1.LocationId.Should().NotBe(forecast2.LocationId);
        forecast1.ForecastDate.Should().NotBe(forecast2.ForecastDate);
        forecast1.Temperature.Should().NotBe(forecast2.Temperature);
    }

    [Fact]
    public void WeatherForecast_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            _validWeatherCode);

        // Assert
        forecast.Should().BeAssignableTo<WeatherForecastAPI.Common.Database.BaseEntity>();
    }

    [Fact]
    public void WeatherForecast_CannotBeCreatedWithoutFactory()
    {
        // Assert - WeatherForecast has private constructor
        var constructors = typeof(WeatherForecast).GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        constructors.Should().BeEmpty();
    }

    [Fact]
    public void Create_RetrievedAtIsConsistent()
    {
        // Act
        var forecast1 = WeatherForecast.Create(1, _validForecastDate, _validTemperature, _validWindSpeed, 1);
        System.Threading.Thread.Sleep(10);
        var forecast2 = WeatherForecast.Create(1, _validForecastDate, _validTemperature, _validWindSpeed, 1);

        // Assert
        forecast2.RetrievedAt.Should().BeOnOrAfter(forecast1.RetrievedAt);
    }

    [Fact]
    public void Create_WithMinAndMaxLocationIds_ReturnsWeatherForecast()
    {
        // Arrange
        int minId = 1;
        int maxId = int.MaxValue;

        // Act
        var forecastMin = WeatherForecast.Create(minId, _validForecastDate, _validTemperature, _validWindSpeed, _validWeatherCode);
        var forecastMax = WeatherForecast.Create(maxId, _validForecastDate, _validTemperature, _validWindSpeed, _validWeatherCode);

        // Assert
        forecastMin.LocationId.Should().Be(minId);
        forecastMax.LocationId.Should().Be(maxId);
    }

    [Fact]
    public void Create_WithMaxWeatherCode_ReturnsWeatherForecast()
    {
        // Arrange
        int maxWeatherCode = int.MaxValue;

        // Act
        var forecast = WeatherForecast.Create(
            _validLocationId,
            _validForecastDate,
            _validTemperature,
            _validWindSpeed,
            maxWeatherCode);

        // Assert
        forecast.WeatherCode.Should().Be(maxWeatherCode);
    }
}

using WeatherForecastAPI.Common.Database;
using WeatherForecastAPI.Features.Locations;

namespace WeatherForecastAPI.Features.Weather;

public class WeatherForecast : BaseEntity
{
    public int LocationId { get; private set; }
    public DateOnly ForecastDate { get; private set; }
    public Temperature Temperature { get; private set; } = null!;
    public WindSpeed WindSpeed { get; private set; } = null!;
    public int WeatherCode { get; private set; }
    public DateTimeOffset RetrievedAt { get; private set; }

    public Location Location { get; private set; } = null!;

    private WeatherForecast()
    { }

    public static WeatherForecast Create(
        int locationId,
        DateOnly forecastDate,
        Temperature temperature,
        WindSpeed windSpeed,
        int weatherCode)
    {
        if (locationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(locationId), "Location ID must be positive.");

        if (weatherCode < 0)
            throw new ArgumentOutOfRangeException(nameof(weatherCode), "Weather code must be non-negative.");

        return new WeatherForecast
        {
            LocationId = locationId,
            ForecastDate = forecastDate,
            Temperature = temperature,
            WindSpeed = windSpeed,
            WeatherCode = weatherCode,
            RetrievedAt = DateTimeOffset.UtcNow
        };
    }
}
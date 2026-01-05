using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WeatherForecastAPI.Common.Database;
using WeatherForecastAPI.Common.ExternalClients;
using WeatherForecastAPI.Features.Locations;

namespace WeatherForecastAPI.Features.Weather;

public class GetWeatherByLocationRequest
{
    public int Id { get; set; }
}

public record WeatherForecastResponse(
    int LocationId,
    decimal Latitude,
    decimal Longitude,
    string? Name,
    CurrentWeatherDto? CurrentWeather,
    List<DailyForecastDto> DailyForecasts,
    DateTime RetrievedAt,
    bool FromCache
);

public record CurrentWeatherDto(
    decimal Temperature,
    decimal WindSpeed,
    int WeatherCode
);

public record DailyForecastDto(
    DateOnly Date,
    decimal Temperature,
    decimal MaxTemperature,
    decimal MinTemperature,
    decimal WindSpeed,
    int WeatherCode
);

public class GetWeatherByLocation(
    IOpenMeteoApi weatherApi,
    ApplicationDbContext db,
    ILogger<GetWeatherByLocation> logger)
    : Endpoint<GetWeatherByLocationRequest, WeatherForecastResponse>
{
    private const int CacheExpirationHours = 1;

    public override void Configure()
    {
        Get("/api/weather/locations/{id}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get weather forecast for a saved location";
            s.Description = $"Returns cached forecast if fresh (< {CacheExpirationHours} hour), otherwise fetches from Open-Meteo API and updates the database";
            s.Response(200, "Weather forecast retrieved");
            s.Response(404, "Location not found");
            s.Response(503, "Weather service unavailable");
        });
    }

    public override async Task HandleAsync(GetWeatherByLocationRequest req, CancellationToken ct)
    {
        var location = await db.Locations
            .Include(l => l.WeatherForecasts)
            .FirstOrDefaultAsync(l => l.Id == req.Id, ct);

        if (location == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        location.UpdateUsage();

        var shouldRefetch = ShouldRefetchForecast(location.WeatherForecasts);

        if (shouldRefetch)
        {
            logger.LogInformation(
                "Cache miss or expired for location {LocationId}. Fetching from API",
                location.Id);

            var weatherData = await weatherApi.GetForecastAsync(
                location.Coordinates.Latitude,
                location.Coordinates.Longitude,
                ct: ct);

            db.WeatherForecasts.RemoveRange(location.WeatherForecasts);

            var newForecasts = MapApiResponseToForecasts(location.Id, weatherData);
            db.WeatherForecasts.AddRange(newForecasts);

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Successfully stored fresh weather data for location {LocationId}",
                location.Id);

            var response = MapToResponse(location, newForecasts, fromCache: false);
            await SendAsync(response, cancellation: ct);
        }
        else
        {
            logger.LogInformation(
                "Cache hit for location {LocationId}",
                location.Id);

            await db.SaveChangesAsync(ct); 

            var response = MapToResponse(location, location.WeatherForecasts.ToList(), fromCache: true);
            await SendAsync(response, cancellation: ct);
        }
    }

    private static bool ShouldRefetchForecast(IReadOnlyCollection<WeatherForecast> forecasts)
    {
        if (!forecasts.Any())
            return true;

        var latestRetrievedAt = forecasts.Max(f => f.RetrievedAt);
        var cacheAge = DateTime.UtcNow - latestRetrievedAt;

        return cacheAge > TimeSpan.FromHours(CacheExpirationHours);
    }

    private static List<WeatherForecast> MapApiResponseToForecasts(
        int locationId,
        OpenMeteoResponse weatherData)
    {
        var forecasts = new List<WeatherForecast>();

        for (int i = 0; i < weatherData.Daily.Time.Count; i++)
        {
            var date = DateOnly.Parse(weatherData.Daily.Time[i]);
            var avgTemp = (weatherData.Daily.MaxTemperatures[i] +
                          weatherData.Daily.MinTemperatures[i]) / 2;

            var temperature = Temperature.Create(
                current: avgTemp,
                maximum: weatherData.Daily.MaxTemperatures[i],
                minimum: weatherData.Daily.MinTemperatures[i]
            );

            var windSpeed = WindSpeed.Create(weatherData.Daily.MaxWindSpeeds[i]);

            var forecast = WeatherForecast.Create(
                locationId: locationId,
                forecastDate: date,
                temperature: temperature,
                windSpeed: windSpeed,
                weatherCode: weatherData.Daily.WeatherCodes[i]
            );

            forecasts.Add(forecast);
        }

        return forecasts;
    }

    private static WeatherForecastResponse MapToResponse(
        Location location,
        List<WeatherForecast> forecasts,
        bool fromCache)
    {
        var dailyForecasts = forecasts
            .OrderBy(wf => wf.ForecastDate)
            .Select(wf => new DailyForecastDto(
                Date: wf.ForecastDate,
                Temperature: wf.Temperature.Current,
                MaxTemperature: wf.Temperature.Maximum,
                MinTemperature: wf.Temperature.Minimum,
                WindSpeed: wf.WindSpeed.Value,
                WeatherCode: wf.WeatherCode
            ))
            .ToList();

        var retrievedAt = forecasts.Any()
            ? forecasts.Max(wf => wf.RetrievedAt)
            : DateTime.UtcNow;

        return new WeatherForecastResponse(
            LocationId: location.Id,
            Latitude: location.Coordinates.Latitude,
            Longitude: location.Coordinates.Longitude,
            Name: location.Name,
            CurrentWeather: null,
            DailyForecasts: dailyForecasts,
            RetrievedAt: retrievedAt,
            FromCache: fromCache
        );
    }
}
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WeatherForecastAPI.Common.Database;
using WeatherForecastAPI.Common.ExternalClients;
using WeatherForecastAPI.Features.Locations;

namespace WeatherForecastAPI.Features.Weather;

public record WeatherByTargetRequest(string Target);

public class GetWeatherByTarget(
    IIpApiService ipApi,
    IOpenMeteoApi weatherApi,
    ApplicationDbContext db) : Endpoint<WeatherByTargetRequest, WeatherForecastResponse>
{
    private const int CacheExpirationHours = 1;

    public override void Configure()
    {
        Get("/weather/by-target/{target}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get weather by IP or URL";
            s.Description = "Geolocates the IP/URL, saves/updates location, and stores newest forecast.";
            s.Response(200, "Forecast retrieved and stored successfully.");
            s.Response(400, "Invalid target or geolocation failed.");
        });
    }

    public override async Task HandleAsync(WeatherByTargetRequest req, CancellationToken ct)
    {
        var geo = await ipApi.GetLocationAsync(req.Target, ct);
        if (geo.Status != "success")
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        var coords = Coordinates.Create(geo.Latitude, geo.Longitude);

        var location = await db.Locations
            .FirstOrDefaultAsync(l => l.Coordinates.Latitude == coords.Latitude &&
                                     l.Coordinates.Longitude == coords.Longitude, ct);

        if (location is null)
        {
            location = Location.Create(coords, $"{geo.City}");
            db.Locations.Add(location);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            location.UpdateUsage();
        }
        var shouldRefetch = ShouldRefetchForecast(location.WeatherForecasts);
        List<WeatherForecast> forecastsToReturn;

        if (shouldRefetch)
        {
            var weatherData = await weatherApi.GetForecastAsync(coords.Latitude, coords.Longitude, ct: ct);

            await db.WeatherForecasts
                    .Where(f => f.LocationId == location.Id)
                    .ExecuteDeleteAsync(ct);

            forecastsToReturn = MapApiResponseToForecasts(location.Id, weatherData);
            db.WeatherForecasts.AddRange(forecastsToReturn);

            await db.SaveChangesAsync(ct);
            var response = MapToResponse(location, forecastsToReturn, fromCache: true);
            await SendAsync(response, 200, ct);
        }
        else
        {
            forecastsToReturn = location.WeatherForecasts.ToList();
            await db.SaveChangesAsync(ct);
            var response = MapToResponse(location, forecastsToReturn, fromCache: false);
            await SendAsync(response, 200, ct);
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
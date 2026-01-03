using Refit;
using System.Text.Json.Serialization;

namespace WeatherForecastAPI.Common.ExternalClients;

public interface IOpenMeteoApi
{
    [Get("/v1/forecast")]
    Task<OpenMeteoResponse> GetForecastAsync(
        [AliasAs("latitude")] decimal latitude,
        [AliasAs("longitude")] decimal longitude,
        [AliasAs("daily")] string daily = "temperature_2m_max,temperature_2m_min,weathercode,windspeed_10m_max",
        [AliasAs("current_weather")] bool currentWeather = true,
        [AliasAs("timezone")] string timezone = "auto",
        [AliasAs("forecast_days")] int forecastDays = 7,
        CancellationToken ct = default
    );
}

public record OpenMeteoResponse(
    [property: JsonPropertyName("latitude")] decimal Latitude,
    [property: JsonPropertyName("longitude")] decimal Longitude,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("current_weather")] CurrentWeather? CurrentWeather,
    [property: JsonPropertyName("daily")] DailyData Daily
);

public record CurrentWeather(
    [property: JsonPropertyName("temperature")] decimal Temperature,
    [property: JsonPropertyName("windspeed")] decimal WindSpeed,
    [property: JsonPropertyName("weathercode")] int WeatherCode,
    [property: JsonPropertyName("time")] string Time
);

public record DailyData(
    [property: JsonPropertyName("time")] List<string> Time,
    [property: JsonPropertyName("temperature_2m_max")] List<decimal> MaxTemperatures,
    [property: JsonPropertyName("temperature_2m_min")] List<decimal> MinTemperatures,
    [property: JsonPropertyName("weathercode")] List<int> WeatherCodes,
    [property: JsonPropertyName("windspeed_10m_max")] List<decimal> MaxWindSpeeds
);
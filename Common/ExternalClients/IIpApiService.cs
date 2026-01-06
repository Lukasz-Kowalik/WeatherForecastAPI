using Refit;
using System.Text.Json.Serialization;

namespace WeatherForecastAPI.Common.ExternalClients;

public record IpApiResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("lat")] decimal Latitude,
    [property: JsonPropertyName("lon")] decimal Longitude,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("country")] string Country
);

public interface IIpApiService
{
    [Get("/json/{query}")]
    Task<IpApiResponse> GetLocationAsync(string query, CancellationToken ct);
}
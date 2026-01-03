using WeatherForecastAPI.Common.Database;

namespace WeatherForecastAPI.Features.Locations;

public class Location : BaseEntity
{
    public Coordinates Coordinates { get; private set; } = null!;
    public string? Name { get; private set; }
    public DateTime LastUsedAt { get; private set; }  // Potential good use case for TimeProvider

    private readonly List<Weather.WeatherForecast> _weatherForecasts = new();
    public IReadOnlyCollection<Weather.WeatherForecast> WeatherForecasts => _weatherForecasts.AsReadOnly();

    private Location()
    { }

    public static Location Create(Coordinates coordinates, string? name = null)
    {
        return new Location
        {
            Coordinates = coordinates,
            Name = name,
            LastUsedAt = DateTime.UtcNow
        };
    }

    public void UpdateUsage()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
    }

    public void ClearName()
    {
        Name = null;
    }
}
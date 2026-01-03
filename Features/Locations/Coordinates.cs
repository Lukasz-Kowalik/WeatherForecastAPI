namespace WeatherForecastAPI.Features.Locations;

public record Coordinates
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }

    private Coordinates(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                latitude,
                "Latitude must be between -90 and 90.");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(
                nameof(longitude),
                longitude,
                "Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }

    public static Coordinates Create(decimal latitude, decimal longitude)
        => new(latitude, longitude);
}
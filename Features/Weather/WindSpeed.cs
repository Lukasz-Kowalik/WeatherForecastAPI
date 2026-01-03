namespace WeatherForecastAPI.Features.Weather;

public record WindSpeed
{
    public decimal Value { get; }

    private WindSpeed(decimal value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Wind speed cannot be negative.");

        Value = value;
    }

    public static WindSpeed Create(decimal value) => new(value);
}
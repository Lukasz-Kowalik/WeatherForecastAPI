namespace WeatherForecastAPI.Features.Weather;

public record Temperature
{
    public decimal Current { get; }
    public decimal Maximum { get; }
    public decimal Minimum { get; }

    private Temperature(decimal current, decimal maximum, decimal minimum)
    {
        if (maximum < minimum)
            throw new ArgumentException(
                $"Maximum temperature ({maximum}) cannot be lower than minimum temperature ({minimum}).",
                nameof(maximum));

        Current = current;
        Maximum = maximum;
        Minimum = minimum;
    }

    public static Temperature Create(decimal current, decimal maximum, decimal minimum)
        => new(current, maximum, minimum);
}
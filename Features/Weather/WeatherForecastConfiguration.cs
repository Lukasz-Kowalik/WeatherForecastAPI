using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WeatherForecastAPI.Features.Weather;

public class WeatherForecastConfiguration : IEntityTypeConfiguration<WeatherForecast>
{
    public void Configure(EntityTypeBuilder<WeatherForecast> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.LocationId)
               .IsRequired();

        builder.Property(e => e.ForecastDate)
               .IsRequired();

        builder.Property(e => e.WeatherCode)
               .IsRequired();

        builder.Property(e => e.RetrievedAt)
               .IsRequired();

        builder.OwnsOne(e => e.Temperature, temp =>
        {
            temp.Property(t => t.Current)
                .HasColumnName("Temperature")
                .HasPrecision(5, 2)
                .IsRequired();

            temp.Property(t => t.Maximum)
                .HasColumnName("MaxTemperature")
                .HasPrecision(5, 2)
                .IsRequired();

            temp.Property(t => t.Minimum)
                .HasColumnName("MinTemperature")
                .HasPrecision(5, 2)
                .IsRequired();
        });

        builder.OwnsOne(e => e.WindSpeed, wind =>
        {
            wind.Property(w => w.Value)
                .HasColumnName("WindSpeed")
                .HasPrecision(5, 2)
                .IsRequired();
        });

        builder.HasOne(e => e.Location)
               .WithMany(l => l.WeatherForecasts)
               .HasForeignKey(e => e.LocationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.LocationId, e.ForecastDate })
               .IsUnique()
               .HasDatabaseName("IX_WeatherForecast_Location_Date");
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WeatherForecastAPI.Features.Locations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(e => e.Id);

        builder.OwnsOne(e => e.Coordinates, coords =>
        {
            coords.Property(c => c.Latitude)
                  .HasColumnName("Latitude")
                  .HasPrecision(10, 6)
                  .IsRequired();

            coords.Property(c => c.Longitude)
                  .HasColumnName("Longitude")
                  .HasPrecision(10, 6)
                  .IsRequired();

            coords.HasIndex(c => new { c.Latitude, c.Longitude })
                  .IsUnique()
                  .HasDatabaseName("IX_Location_Coordinates");
        });

        builder.Property(e => e.Name)
               .HasMaxLength(200);

        builder.Property(e => e.LastUsedAt);

        builder.HasMany(l => l.WeatherForecasts)
               .WithOne(w => w.Location)
               .HasForeignKey(w => w.LocationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
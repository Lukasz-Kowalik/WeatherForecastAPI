using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WeatherForecastAPI.Common.Database;

namespace WeatherForecastAPI.Features.Locations;

public class ListLocations(ApplicationDbContext db)
    : EndpointWithoutRequest<List<LocationResponse>>
{
    public override void Configure()
    {
        Get("/locations");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List all saved locations";
            s.Description = "Returns all previously used locations ordered by most recently used";
            s.Response(200, "List of locations");
            s.Response(500, "Internal server error - occurs if the database is unavailable or an unexpected error happens.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var locations = await db.Locations
            .AsNoTracking()
            .OrderByDescending(l => l.LastUsedAt)
            .Select(l => new LocationResponse(
                l.Id,
                l.Coordinates.Latitude,
                l.Coordinates.Longitude,
                l.Name,
                l.CreatedAt,
                l.LastUsedAt
            ))
            .ToListAsync(ct);

        await SendAsync(locations, cancellation: ct);
    }
}
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WeatherForecastAPI.Common.Database;

namespace WeatherForecastAPI.Features.Locations;

public record LocationListResponse(
    List<LocationResponse> Locations,
    int TotalCount
);

public class ListLocations(ApplicationDbContext db) : EndpointWithoutRequest<LocationListResponse>
{
    public override void Configure()
    {
        Get("/locations");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List all stored locations";
            s.Description = "Returns a list of all previously used coordinates and names, ordered by the most recent activity (LastUsedAt or CreatedAt).";
            s.Response(200, "List of locations retrieved successfully.");
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

        await SendAsync(new LocationListResponse(locations, locations.Count), 200, ct);
    }
}
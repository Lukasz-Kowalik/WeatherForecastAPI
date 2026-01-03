using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WeatherForecastAPI.Common.Database;

namespace WeatherForecastAPI.Features.Locations;

public record AddLocationRequest(
    decimal Latitude,
    decimal Longitude,
    string? Name = null
);

public record LocationResponse(
    int Id,
    decimal Latitude,
    decimal Longitude,
    string? Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUsedAt
);

public class AddLocation(ApplicationDbContext db) : Endpoint<AddLocationRequest, LocationResponse>
{
    public override void Configure()
    {
        Post("/locations");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Add a new location";
            s.Description = "Creates a new location or returns existing one if coordinates already exist";
            s.ExampleRequest = new AddLocationRequest(52.2297m, 21.0122m, "Warsaw");

            // Response descriptions
            s.Response(200, "Location already exists, returning existing record");
            s.Response(201, "New location created successfully");
            s.Response(400, "Invalid request - check coordinates range");
            s.Response(500, "Internal server error");
        });
    }

    public class Validator : Validator<AddLocationRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
            RuleFor(x => x.Name).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Name));
        }
    }

    public override async Task HandleAsync(AddLocationRequest req, CancellationToken ct)
    {
        var coordinates = Coordinates.Create(req.Latitude, req.Longitude);

        var existing = await db.Locations
            .FirstOrDefaultAsync(l =>
                l.Coordinates.Latitude == coordinates.Latitude &&
                l.Coordinates.Longitude == coordinates.Longitude, ct);

        if (existing != null)
        {
            await SendAsync(MapToResponse(existing), cancellation: ct);
            return;
        }

        var location = Location.Create(coordinates, req.Name);
        db.Locations.Add(location);
        await db.SaveChangesAsync(ct);

        await SendAsync(MapToResponse(location), 201, ct);
    }

    private static LocationResponse MapToResponse(Location location) =>
        new(
            location.Id,
            location.Coordinates.Latitude,
            location.Coordinates.Longitude,
            location.Name,
            location.CreatedAt,
            location.LastUsedAt
        );
}
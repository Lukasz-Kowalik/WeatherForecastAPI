using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WeatherForecastAPI.Common.Database;

namespace WeatherForecastAPI.Features.Locations;

public class DeleteLocationRequest
{
    public int Id { get; set; }
}

public class DeleteLocation(ApplicationDbContext db) : Endpoint<DeleteLocationRequest>
{
    public override void Configure()
    {
        Delete("/locations/{id}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a location by ID";
            s.Description = "Deletes a location and all associated weather forecasts (cascade delete)";
            s.Response(204, "Location deleted successfully");
            s.Response(404, "Location not found");
        });
    }

    public override async Task HandleAsync(DeleteLocationRequest req, CancellationToken ct)
    {
        var deletedCount = await db.Locations
            .Where(l => l.Id == req.Id)
            .ExecuteDeleteAsync(ct);

        if (deletedCount == 0)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
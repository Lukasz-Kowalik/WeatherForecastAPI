using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WeatherForecastAPI.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

await app.InitializeDatabaseAsync();

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            details = report.Entries.Select(e => new { key = e.Key, status = e.Value.Status.ToString() })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.UseSwaggerGen();

app.Run();

// Make Program accessible for integration tests
public partial class Program
{ }
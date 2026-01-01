using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;
using WeatherForecastAPI.Common.Database;
using WeatherForecastAPI.Common.Exceptions;
using WeatherForecastAPI.Common.ExternalClients;
using WeatherForecastAPI.Features.Locations;

namespace WeatherForecastAPI.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var openMeteoConfig = configuration.GetSection("ExternalServices:OpenMeteo");
        var ipApiConfig = configuration.GetSection("ExternalServices:IpApi");
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString, options => options.CommandTimeout(30)));

        services.AddRefitClient<IOpenMeteoApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(openMeteoConfig["BaseUrl"]!);
                    c.Timeout = TimeSpan.FromSeconds(10);
                })
                .AddStandardResilienceHandler(ConfigureStandardResilience);

        services.AddRefitClient<IIpApiService>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(ipApiConfig["BaseUrl"]!);
                    c.Timeout = TimeSpan.FromSeconds(10);
                })
                .AddStandardResilienceHandler(ConfigureStandardResilience);

        services.AddHealthChecks()
                .AddSqlite(connectionString!, name: "Database")
                .AddUrlGroup(new Uri(openMeteoConfig["HealthCheckUrl"]!), name: "Open-Meteo API")
                .AddUrlGroup(new Uri(ipApiConfig["HealthCheckUrl"]!), name: "IP-Geolocation API");

        // FastEndpoints
        services.AddFastEndpoints();
        services.SwaggerDocument(o =>
        {
            o.DocumentSettings = s =>
            {
                s.Title = "Weather Forecast API";
                s.Version = "v1";
                s.Description = "API for storing and retrieving weather forecasts";
            };
        });

        // Exception Handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();


        return services;
    }

    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();

        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
        await db.Database.ExecuteSqlRawAsync("PRAGMA synchronous=NORMAL;");
        await db.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=5000;");

        if (!await db.Locations.AnyAsync())
        {
            db.Locations.AddRange(
                new Location { Latitude = 52.2297m, Longitude = 21.0122m, Name = "Warsaw" },
                new Location { Latitude = 51.5074m, Longitude = -0.1278m, Name = "London" },
                new Location { Latitude = 40.7128m, Longitude = -74.0060m, Name = "New York" }
            );

            await db.SaveChangesAsync();
        }

        return app;
    }
    private static void ConfigureStandardResilience(HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);

        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.Delay = TimeSpan.FromSeconds(1);

        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 10;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    }

}

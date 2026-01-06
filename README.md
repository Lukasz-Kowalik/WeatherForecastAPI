# Weather Forecast API (.NET 10)

A robust, resilient RESTful API for managing weather locations and forecasts. Built with a focus on high availability, error handling, and pragmatic architecture.

## 🚀 Quick Start (Docker)

The application is fully containerized. To get the system up and running with all dependencies and seed data:

```bash
docker compose up --build
```

- **API Base URL:** http://localhost:5000
- **Interactive Documentation (Swagger):** http://localhost:5000/swagger
- **Health Monitoring:** http://localhost:5000/health

## 🏗 Architecture: Vertical Slice (VSA)

Instead of traditional, over-engineered 4-project Clean Architecture, this solution uses **Vertical Slice Architecture**.

### Reasoning
For a service of this scale, grouping logic by feature (Locations, Weather) rather than technical layers (Repository, Service, etc.) significantly increases maintainability and readability.

### KISS/YAGNI Principles
- No redundant abstractions
- Database access is performed directly in handlers using EF Core, which is a mature implementation of the Unit of Work/Repository patterns
- Feature-based folder structure makes navigation intuitive

## 🛡 Resilience & "Unfortunate Conditions"

The core of this task was handling external failures. This solution implements:

### Fault Tolerance (Polly)
All external calls (Open-Meteo & IP-API) are wrapped in a **StandardResilienceHandler**:
- **Retry Policy:** Exponential backoff for transient errors
- **Circuit Breaker:** Stops requests to failing external services to prevent resource exhaustion
- **Timeout:** Strict execution limits to prevent "hanging" requests

### Database Robustness (SQLite)
- **WAL Mode (Write-Ahead Logging):** Enabled to allow concurrent reads and writes, crucial for SQLite stability
- **Busy Timeout:** Configured to 5 seconds to handle file-locking during high load
- **Atomic Cache Invalidation:** Uses `ExecuteDeleteAsync` to clear expired forecasts, avoiding UNIQUE constraint conflicts during high-concurrency updates

### Global Error Handling
- Custom `GlobalExceptionHandler` mapping exceptions to RFC 7807 Problem Details
- Ensures no sensitive stack traces are leaked
- Provides clear, structured feedback to clients

## 🌐 IP & URL Geolocation

The API supports fetching weather by IP address or Domain Name (URL) via the `GET /api/weather/by-target/{target}` endpoint.

### How it works
Uses **ip-api.com** (a reliable, no-key-required alternative to IPStack) to resolve coordinates before fetching weather data.

### Resilience
This secondary API is also covered by its own circuit breaker and health checks.

## 💾 Caching Strategy

To respect Open-Meteo's rate limits and improve performance:
- Forecasts are cached in the local database for **1 hour**
- Subsequent requests for the same location within this window are served from SQLite
- Response includes a `fromCache: true` flag to indicate cached data

## 🧪 API Endpoints

### Locations
- `GET /api/locations` - List all locations (ordered by most recently used)
- `POST /api/locations` - Add a new location
- `DELETE /api/locations/{id}` - Delete a location by ID

### Weather
- `GET /api/weather/locations/{id}` - Get weather for a specific location
- `GET /api/weather/by-target/{target}` - Get weather by IP address or domain name

### Health & Monitoring
- `GET /health` - Health check endpoint
- `GET /swagger` - Interactive API documentation

## Testing

### Unit Tests
Focus on domain invariants (e.g., coordinate ranges, temperature logic).

### Integration Tests
Uses `WebApplicationFactory` with an In-Memory SQLite provider to test the full HTTP flow without side effects.

### Run tests via CLI
```bash
dotnet test
```

## 🛠 Tech Stack

| Component | Technology |
|-----------|-----------|
| **Runtime** | .NET 10 (latest) |
| **Web Framework** | FastEndpoints (faster and cleaner than Controllers) |
| **Database** | EF Core + SQLite |
| **HTTP Client** | Refit (type-safe REST clients) |
| **Resilience** | Microsoft.Extensions.Http.Resilience (Polly) |
| **Validation** | FluentValidation |
| **Testing** | xUnit + FluentAssertions |

## Project Structure

```
WeatherForecastAPI/
├── Features/
│   ├── Locations/
│   │   ├── AddLocation.cs
│   │   ├── DeleteLocation.cs
│   │   ├── ListLocations.cs
│   │   ├── Location.cs
│   │   └── LocationConfiguration.cs
│   └── Weather/
│       ├── GetWeatherByLocation.cs
│       ├── GetWeatherByTarget.cs
│       ├── WeatherForecast.cs
│       └── WeatherForecastConfiguration.cs
├── Common/
│   ├── Database/
│   │   ├── ApplicationDbContext.cs
│   │   └── BaseEntity.cs
│   ├── ExternalClients/
│   │   ├── IOpenMeteoApi.cs
│   │   └── IIpApiService.cs
│   ├── Exceptions/
│   │   └── GlobalExceptionHandler.cs
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
├── Tests/
│   ├── WeatherForecastAPI_IntegrationTests/
│   │   └── Features/
│   └── WeatherForecastAPI_UnitTests/
│       └── Features/
├── Program.cs
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## Database Seeding

The application comes pre-seeded with 3 locations:
- **Warsaw** (52.2297°N, 21.0122°E)
- **London** (51.5074°N, 0.1278°W)
- **New York** (40.7128°N, 74.0060°W)

Seed data is initialized in `ServiceCollectionExtensions.InitializeDatabaseAsync()`.

## Error Handling

All errors are returned in RFC 7807 Problem Details format:

```json
{
  "type": "https://api.example.com/errors/validation-error",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Request validation failed",
  "errors": {
    "Latitude": ["Latitude must be between -90 and 90"]
  }
}
```

## Configuration

Environment variables (see `docker-compose.yml`):
- `ASPNETCORE_ENVIRONMENT` - Development/Production mode
- `ConnectionStrings__DefaultConnection` - SQLite connection string
- `ExternalServices__OpenMeteo__BaseUrl` - Open-Meteo API base URL
- `ExternalServices__IpApi__BaseUrl` - IP-API base URL

## Contributing

When adding new features:
1. Follow the Vertical Slice pattern - create a feature folder with endpoint, request/response, and configuration
2. Add validation using FluentValidation
3. Include both unit and integration tests
4. Update this README with new endpoints

## License

This project is provided as-is for educational and development purposes.

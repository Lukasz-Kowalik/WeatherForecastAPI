using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace WeatherForecastAPI.Common.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error occurred: {Message}", exception.Message);

        var statusCode = exception switch
        {
            ArgumentException or ArgumentOutOfRangeException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            BrokenCircuitException => StatusCodes.Status503ServiceUnavailable,
            HttpRequestException => StatusCodes.Status502BadGateway,
            TaskCanceledException when !cancellationToken.IsCancellationRequested => StatusCodes.Status504GatewayTimeout,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                503 => "External Service Unavailable (Circuit Breaker)",
                504 => "External Service Timeout",
                _ => "An error occurred"
            },
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
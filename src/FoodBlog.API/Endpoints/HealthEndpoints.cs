using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.API.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health/live", () =>
            Results.Ok(new HealthResponse(
                "Healthy",
                "live",
                DateTimeOffset.UtcNow,
                new Dictionary<string, HealthCheckResult>())))
            .WithName("LiveHealthCheck")
            .WithTags("Health");

        app.MapGet("/health/ready", CheckReadinessAsync)
            .WithName("ReadyHealthCheck")
            .WithTags("Health");

        app.MapGet("/health", CheckHealthAsync)
            .WithName("HealthCheck")
            .WithTags("Health");
    }

    private static Task<IResult> CheckHealthAsync(
        FoodBlogDbContext db,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
        => CheckReadinessAsync(db, loggerFactory.CreateLogger("HealthEndpoints"), cancellationToken);

    private static async Task<IResult> CheckReadinessAsync(
        FoodBlogDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var checks = new Dictionary<string, HealthCheckResult>();
        var startedAt = DateTimeOffset.UtcNow;

        try
        {
            var databaseIsAvailable = await db.Database.CanConnectAsync(cancellationToken);
            checks["database"] = databaseIsAvailable
                ? new HealthCheckResult("Healthy")
                : new HealthCheckResult("Unhealthy", "Database is unavailable.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Database readiness check failed.");
            checks["database"] = new HealthCheckResult("Unhealthy", "Database check failed.");
        }

        var isHealthy = checks.Values.All(check => check.Status == "Healthy");
        var response = new HealthResponse(
            isHealthy ? "Healthy" : "Unhealthy",
            "ready",
            startedAt,
            checks);

        return isHealthy
            ? Results.Ok(response)
            : Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private sealed record HealthResponse(
        string Status,
        string Role,
        DateTimeOffset CheckedAt,
        IReadOnlyDictionary<string, HealthCheckResult> Checks);

    private sealed record HealthCheckResult(
        string Status,
        string? Detail = null);
}

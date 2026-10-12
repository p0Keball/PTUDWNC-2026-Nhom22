using System.Net.Sockets;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.API.Endpoints;

public static class HealthEndpoints
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(3) };

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
        IConfiguration config,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
        => CheckReadinessAsync(db, config, loggerFactory, cancellationToken);

    private static async Task<IResult> CheckReadinessAsync(
        FoodBlogDbContext db,
        IConfiguration config,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("HealthEndpoints");
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

        checks["postgres"] = checks["database"] switch
        {
            { Status: "Healthy" } => new HealthCheckResult("Healthy"),
            var d => new HealthCheckResult("Unhealthy", d.Detail)
        };

        await CheckAsync("redis", () => CheckRedisAsync(config), checks, logger, cancellationToken);
        await CheckAsync("minio", () => CheckMinioAsync(config), checks, logger, cancellationToken);

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

    private static async Task CheckAsync(
        string name,
        Func<Task<bool>> check,
        Dictionary<string, HealthCheckResult> checks,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var ok = await check();
            checks[name] = ok
                ? new HealthCheckResult("Healthy")
                : new HealthCheckResult("Unhealthy", $"{name} is unavailable.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "{Service} readiness check failed.", name);
            checks[name] = new HealthCheckResult("Unhealthy", $"{name} check failed.");
        }
    }

    private static async Task<bool> CheckRedisAsync(IConfiguration config)
    {
        var (host, port) = ParseHostPort(config["Redis:Connection"] ?? "localhost:6379");
        using var client = new TcpClient();
        await client.ConnectAsync(host, port).WaitAsync(TimeSpan.FromSeconds(3));
        return client.Connected;
    }

    private static async Task<bool> CheckMinioAsync(IConfiguration config)
    {
        var endpoint = (config["MinIO:Endpoint"] ?? "http://localhost:9000").TrimEnd('/');
        if (!endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            endpoint = $"http://{endpoint}";
        using var response = await Http.GetAsync($"{endpoint}/minio/health/live");
        return response.IsSuccessStatusCode;
    }

    private static (string Host, int Port) ParseHostPort(string connection)
    {
        var parts = connection.Split(':');
        return parts.Length == 2 && int.TryParse(parts[1], out var port)
            ? (parts[0], port)
            : ("localhost", 6379);
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

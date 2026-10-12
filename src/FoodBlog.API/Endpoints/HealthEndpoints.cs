using System.Net.Sockets;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.API.Endpoints;

public static class HealthEndpoints
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(3) };

    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }))
            .WithName("HealthLive");

        app.MapGet("/health/ready", async (FoodBlogDbContext db, IConfiguration config) =>
        {
            var checks = new Dictionary<string, string>();
            var healthy = true;

            try
            {
                healthy &= await CheckAsync("postgres", () => db.Database.CanConnectAsync(), checks);
                healthy &= await CheckAsync("redis", () => CheckRedisAsync(config), checks);
                healthy &= await CheckAsync("minio", () => CheckMinioAsync(config), checks);
            }
            catch (Exception ex)
            {
                checks["error"] = ex.Message;
                healthy = false;
            }

            var payload = new { status = healthy ? "Healthy" : "Unhealthy", checks };
            return healthy ? Results.Ok(payload) : Results.Json(payload, statusCode: 503);
        }).WithName("HealthReady");
    }

    private static async Task<bool> CheckAsync(string name, Func<Task<bool>> check, Dictionary<string, string> checks)
    {
        try
        {
            var ok = await check();
            checks[name] = ok ? "Healthy" : "Unhealthy";
            return ok;
        }
        catch (Exception ex)
        {
            checks[name] = $"Unhealthy: {ex.Message}";
            return false;
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
        var endpoint = (config["Minio:Endpoint"] ?? "http://localhost:9000").TrimEnd('/');
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
}

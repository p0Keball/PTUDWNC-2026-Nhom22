using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace FoodBlog.API.Middlewares;

/// <summary>
/// NFR-SEC-003: giới hạn request theo IP và trả về đầy đủ header chuẩn
/// <c>X-RateLimit-Limit</c>, <c>X-RateLimit-Remaining</c>, <c>X-RateLimit-Reset</c>,
/// <c>Retry-After</c> cho cả request thành công lẫn request bị từ chối.
///
/// Middleware tự đếm theo cửa sổ cố định (fixed window) thay vì dùng
/// <c>UseRateLimiter()</c> vì middleware built-in không cung cấp số request còn lại
/// cho response thành công.
/// </summary>
public sealed class RateLimitHeadersMiddleware
{
    private const string LimitHeader = "X-RateLimit-Limit";
    private const string RemainingHeader = "X-RateLimit-Remaining";
    private const string ResetHeader = "X-RateLimit-Reset";

    private static readonly IReadOnlyDictionary<string, RateLimitPolicy> Policies =
        new Dictionary<string, RateLimitPolicy>(StringComparer.OrdinalIgnoreCase)
        {
            ["general"] = new("general", PermitLimit: 100, Window: TimeSpan.FromMinutes(1)),
            ["auth"] = new("auth", PermitLimit: 10, Window: TimeSpan.FromMinutes(1)),
            ["upload"] = new("upload", PermitLimit: 5, Window: TimeSpan.FromMinutes(1)),
            ["recipe-write"] = new("recipe-write", PermitLimit: 30, Window: TimeSpan.FromMinutes(1))
        };

    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitHeadersMiddleware> _logger;

    public RateLimitHeadersMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitHeadersMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var policy = ResolvePolicy(context);
        if (policy is null)
        {
            await _next(context);
            return;
        }

        var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTimeOffset.UtcNow;

        var counter = _cache.GetOrCreate(
            $"rate-limit:{policy.Name}:{partitionKey}",
            entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = policy.Window * 2;
                return new WindowCounter(now);
            })!;

        int requestCount;
        DateTimeOffset windowEnd;
        lock (counter)
        {
            // Fixed window bắt đầu từ request đầu tiên của mỗi partition thay vì
            // căn theo phút đồng hồ. Cách này tránh việc cửa sổ reset giữa chừng
            // khi request đến sát mốc phút và giúp hành vi dễ dự đoán hơn.
            if (now - counter.WindowStart >= policy.Window)
            {
                counter.WindowStart = now;
                counter.Count = 0;
            }

            windowEnd = counter.WindowStart.Add(policy.Window);
            requestCount = ++counter.Count;
        }

        var remaining = Math.Max(policy.PermitLimit - requestCount, 0);
        var resetSeconds = Math.Max(
            (long)Math.Ceiling((windowEnd - now).TotalSeconds),
            1);

        context.Response.Headers[LimitHeader] =
            policy.PermitLimit.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers[RemainingHeader] =
            remaining.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers[ResetHeader] =
            windowEnd.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        if (requestCount <= policy.PermitLimit)
        {
            await _next(context);
            return;
        }

        context.Response.Headers.RetryAfter =
            resetSeconds.ToString(CultureInfo.InvariantCulture);

        _logger.LogWarning(
            "Rate limit exceeded for policy {Policy} from {RemoteIp}. Limit {Limit}/{Window}.",
            policy.Name,
            partitionKey,
            policy.PermitLimit,
            policy.Window);

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests.",
                Detail = "Request rate limit exceeded. Please try again later."
            },
            context.RequestAborted);
    }

    private static RateLimitPolicy? ResolvePolicy(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
            return null;

        if (endpoint.Metadata.GetMetadata<DisableRateLimitingAttribute>() is not null)
            return null;

        var policyName = endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;
        if (string.IsNullOrWhiteSpace(policyName))
            return null;

        return Policies.GetValueOrDefault(policyName);
    }

    private sealed class WindowCounter(DateTimeOffset windowStart)
    {
        public DateTimeOffset WindowStart { get; set; } = windowStart;
        public int Count { get; set; }
    }

    private sealed record RateLimitPolicy(string Name, int PermitLimit, TimeSpan Window);
}

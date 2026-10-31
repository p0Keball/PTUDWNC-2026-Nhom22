using Serilog.Context;

namespace FoodBlog.API.Middlewares;

/// <summary>
/// FR-OBS-002 — mỗi request có một CorrelationId (nhận từ header hoặc sinh mới),
/// trả lại trong response header và gắn vào toàn bộ log context.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = context.TraceIdentifier;

        context.TraceIdentifier = correlationId;
        context.Items[ItemKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Bổ sung UserId vào log context sau khi Authentication đã chạy.
/// </summary>
public sealed class UserLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserLogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
              ?? context.User.FindFirst("sub")?.Value
            : null;

        using (LogContext.PushProperty("UserId", userId))
        {
            await _next(context);
        }
    }
}

using System.Security.Claims;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FoodBlog.Infrastructure.Services;

/// <summary>
/// Ghi audit log vào bảng AuditLogs và đồng thời phát một log entry có cấu trúc
/// để Seq/File sink có thể đối chiếu. Toàn bộ thao tác là best-effort.
/// </summary>
public sealed class AuditLogger : IAuditLogger
{
    private readonly FoodBlogDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(
        FoodBlogDbContext db,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogger> logger)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var http = _httpContextAccessor.HttpContext;
        var principal = http?.User;
        var userId = entry.UserId
            ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal?.FindFirstValue("sub");
        var userEmail = entry.UserEmail
            ?? principal?.FindFirstValue(ClaimTypes.Email)
            ?? principal?.FindFirstValue("email");

        var log = new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Description = entry.Description,
            IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Truncate(http?.Request.Headers.UserAgent.ToString(), 512),
            CorrelationId = http?.Items["CorrelationId"] as string ?? http?.TraceIdentifier,
            Succeeded = entry.Succeeded,
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Audit không được phép làm hỏng request nghiệp vụ.
            _logger.LogError(ex,
                "Không ghi được audit log {Action} cho {EntityType}/{EntityId}",
                entry.Action, entry.EntityType, entry.EntityId);
        }

        _logger.LogInformation(
            "AUDIT {Action} {EntityType}/{EntityId} by {UserId} ({UserEmail}) success={Succeeded}: {Description}",
            log.Action, log.EntityType, log.EntityId, log.UserId, log.UserEmail, log.Succeeded, log.Description);
    }

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Length <= max ? value : value[..max];
    }
}

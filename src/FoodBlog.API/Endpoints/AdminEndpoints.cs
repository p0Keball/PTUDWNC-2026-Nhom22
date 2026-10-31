using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.API.Endpoints;

public record SetUserStatusRequest(bool IsActive);
public record SetUserRoleRequest(string Role);

/// <summary>
/// Khu vực quản trị: danh sách người dùng (Admin/Author) và nhật ký hệ thống.
/// Toàn bộ endpoint yêu cầu role Admin.
/// </summary>
public static class AdminEndpoints
{
    private static readonly string[] AssignableRoles = ["Admin", "Author"];

    public static void MapAdminEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/admin")
            .WithApiVersionSet(apiVersionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .RequireRateLimiting("general");

        group.MapGet("/users", GetUsers);
        group.MapPatch("/users/{id}/status", SetUserStatus);
        group.MapPatch("/users/{id}/role", SetUserRole);
        group.MapGet("/audit-logs", GetAuditLogs);
    }

    private static async Task<IResult> GetUsers(
        FoodBlogDbContext db,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? role = null,
        string? status = null,
        string sort = "-createdAt",
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                u.DisplayName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleName = role.Trim();
            query = query.Where(u => db.UserRoles
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .Any(x => x.UserId == u.Id && x.Name == roleName));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => u.IsActive);
            else if (status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => !u.IsActive);
        }

        var totalCount = await query.CountAsync(ct);

        query = sort?.Trim() switch
        {
            "name" => query.OrderBy(u => u.DisplayName),
            "-name" => query.OrderByDescending(u => u.DisplayName),
            "email" => query.OrderBy(u => u.Email),
            "createdAt" => query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt),
        };

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.DisplayName,
                u.AvatarUrl,
                u.IsActive,
                u.CreatedAt,
                u.LockoutEnd,
            })
            .ToListAsync(ct);

        var userIds = users.Select(u => u.Id).ToList();

        var roleMap = await db.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, Role = r.Name! })
            .ToListAsync(ct);

        var recipeCounts = await db.Recipes
            .Where(r => userIds.Contains(r.AuthorId) && !r.IsDeleted)
            .GroupBy(r => r.AuthorId)
            .Select(g => new { AuthorId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AuthorId, x => x.Count, ct);

        var items = users.Select(u =>
        {
            var roles = roleMap.Where(r => r.UserId == u.Id).Select(r => r.Role).ToArray();
            return new
            {
                u.Id,
                u.Email,
                u.DisplayName,
                u.AvatarUrl,
                roles = roles.Length > 0 ? roles : ["Author"],
                role = roles.FirstOrDefault() ?? "Author",
                u.IsActive,
                isLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                u.CreatedAt,
                recipeCount = recipeCounts.GetValueOrDefault(u.Id),
            };
        }).ToArray();

        var stats = await GetUserStatsAsync(db, ct);

        return Results.Ok(new { items, totalCount, page, pageSize, stats });
    }

    private static async Task<object> GetUserStatsAsync(FoodBlogDbContext db, CancellationToken ct)
    {
        var totalUsers = await db.Users.CountAsync(ct);
        var activeUsers = await db.Users.CountAsync(u => u.IsActive, ct);
        var admins = await CountUsersInRoleAsync(db, "Admin", ct);
        var authors = await CountUsersInRoleAsync(db, "Author", ct);
        return new { totalUsers, activeUsers, admins, authors };
    }

    private static Task<int> CountUsersInRoleAsync(FoodBlogDbContext db, string role, CancellationToken ct) =>
        db.UserRoles
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .CountAsync(name => name == role, ct);

    private static async Task<IResult> SetUserStatus(
        string id,
        SetUserStatusRequest request,
        UserManager<ApplicationUser> users,
        FoodBlogDbContext db,
        IAuditLogger audit,
        ClaimsPrincipal principal,
        CancellationToken ct)
    {
        var actorId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        if (actorId == id && !request.IsActive)
            return Results.UnprocessableEntity(new
            {
                code = "ADMIN_SELF_DEACTIVATE",
                title = "Không thể tự vô hiệu hóa tài khoản đang đăng nhập.",
            });

        var user = await users.FindByIdAsync(id);
        if (user is null)
            return Results.NotFound(new { code = "USER_NOT_FOUND", title = "Không tìm thấy người dùng.", id });

        if (user.IsActive == request.IsActive)
            return Results.Ok(new { user.Id, user.IsActive });

        user.IsActive = request.IsActive;

        // Vô hiệu hóa tài khoản: thu hồi toàn bộ refresh token để chấm dứt phiên.
        if (!request.IsActive)
        {
            var tokens = await db.RefreshTokens.Where(t => t.UserId == id).ToListAsync(ct);
            if (tokens.Count > 0)
                db.RefreshTokens.RemoveRange(tokens);
        }

        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
            return Results.UnprocessableEntity(new
            {
                title = "Không cập nhật được trạng thái người dùng.",
                errors = result.Errors.GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()),
            });

        await audit.LogAsync(new AuditLogEntry(
            request.IsActive ? "USER_ACTIVATED" : "USER_DEACTIVATED",
            "User", user.Id, request.IsActive ? $"Kích hoạt tài khoản {user.Email}" : $"Vô hiệu hóa tài khoản {user.Email}"), ct);

        return Results.Ok(new { user.Id, user.IsActive });
    }

    private static async Task<IResult> SetUserRole(
        string id,
        SetUserRoleRequest request,
        UserManager<ApplicationUser> users,
        IAuditLogger audit,
        ClaimsPrincipal principal,
        CancellationToken ct)
    {
        var role = request.Role?.Trim() ?? string.Empty;
        if (!AssignableRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            return Results.UnprocessableEntity(new
            {
                code = "USER_INVALID_ROLE",
                title = "Role không hợp lệ.",
                errors = new { role = new[] { "Chỉ chấp nhận Admin hoặc Author." } },
            });

        var actorId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        if (actorId == id)
            return Results.UnprocessableEntity(new
            {
                code = "ADMIN_SELF_ROLE_CHANGE",
                title = "Không thể tự thay đổi role của chính mình.",
            });

        var user = await users.FindByIdAsync(id);
        if (user is null)
            return Results.NotFound(new { code = "USER_NOT_FOUND", title = "Không tìm thấy người dùng.", id });

        var currentRoles = await users.GetRolesAsync(user);
        var canonicalRole = AssignableRoles.First(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        if (currentRoles.Count == 1 && currentRoles[0] == canonicalRole)
            return Results.Ok(new { user.Id, role = canonicalRole });

        var removeResult = await users.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return Results.Problem(title: "Không thể gỡ role cũ.", statusCode: StatusCodes.Status500InternalServerError);

        if (!await users.IsInRoleAsync(user, canonicalRole))
        {
            var addResult = await users.AddToRoleAsync(user, canonicalRole);
            if (!addResult.Succeeded)
                return Results.Problem(title: "Không thể gán role mới.", statusCode: StatusCodes.Status500InternalServerError);
        }

        await audit.LogAsync(new AuditLogEntry(
            "USER_ROLE_CHANGED", "User", user.Id,
            $"Đổi role {user.Email}: {string.Join(", ", currentRoles)} → {canonicalRole}"), ct);

        return Results.Ok(new { user.Id, role = canonicalRole });
    }

    private static async Task<IResult> GetAuditLogs(
        FoodBlogDbContext db,
        int page = 1,
        int pageSize = 20,
        string? action = null,
        string? entityType = null,
        string? userId = null,
        string? search = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action == action);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(l => l.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(l => l.UserId == userId);
        if (from.HasValue)
            query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.CreatedAt <= to.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(l =>
                l.Description.ToLower().Contains(term) ||
                (l.UserEmail != null && l.UserEmail.ToLower().Contains(term)) ||
                (l.EntityId != null && l.EntityId.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.Id,
                l.UserId,
                l.UserEmail,
                l.Action,
                l.EntityType,
                l.EntityId,
                l.Description,
                l.IpAddress,
                l.CorrelationId,
                l.Succeeded,
                l.CreatedAt,
            })
            .ToListAsync(ct);

        return Results.Ok(new { items, totalCount, page, pageSize });
    }
}

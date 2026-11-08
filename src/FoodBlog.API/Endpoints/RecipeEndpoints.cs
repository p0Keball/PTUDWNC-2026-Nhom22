using System.Security.Claims;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Features.Recipes;
using Asp.Versioning;
using Asp.Versioning.Builder;
using FoodBlog.Application.Features.Recipes.Commands;
using FoodBlog.Application.Features.Recipes.Queries;
using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Domain.Exceptions;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.OutputCaching;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace FoodBlog.API.Endpoints;

public record UpdateRecipeRequest(string Title, string Description, string Instructions, Guid CategoryId,
    int PrepTimeMinutes, int CookTimeMinutes, int Servings, int Difficulty, string RowVersion,
    NutritionDto? Nutrition = null);

/// <summary>
/// Projection dùng cho raw SQL FTS (websearch_to_tsquery + ts_rank).
/// EF Core yêu cầu property settable và khớp tên cột trong câu SELECT.
/// </summary>
public sealed class RecipeSearchRow
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public int Difficulty { get; set; }
    public int Status { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/recipes")
            .WithApiVersionSet(apiVersionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Recipes")
            .RequireRateLimiting("general");

        group.MapGet("/", async (ISender sender, HttpContext http,
            int page = 1, int pageSize = 12, Guid? categoryId = null,
            int? difficulty = null, int? maxCookTime = null, int? minServings = null,
            string sort = "-createdAt") =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? http.User.FindFirstValue("sub");
            var isAdmin = http.User.IsInRole("Admin");
            var result = await sender.Send(new GetRecipesQuery(
                page, pageSize, categoryId, difficulty, maxCookTime, minServings,
                sort, userId, isAdmin));
            return Results.Ok(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                page = result.Page,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                hasNextPage = result.HasNextPage,
                hasPreviousPage = result.HasPreviousPage
            });
        }).CacheOutput("RecipeList");

        group.MapGet("/search", async (FoodBlogDbContext db, string? q, int page = 1, int pageSize = 12,
            Guid? categoryId = null, int? difficulty = null, int? maxCookTime = null,
            int? minServings = null, string sort = "-createdAt") =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.UnprocessableEntity(new { title = "Thiếu từ khóa tìm kiếm.", errors = new { q = new[] { "q là bắt buộc." } } });
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var term = q.Trim();

            // FR-SRCH-001 / FR-SRCH-003: PostgreSQL FTS + unaccent, cấu hình 'simple'
            // phù hợp tiếng Việt (không stemming tiếng Anh), xếp hạng bằng ts_rank().
            // Các bộ lọc/sắp xếp được dựng bằng placeholder {n} để tham số hóa an toàn.
            var parameters = new List<object> { term };
            var conditions = new List<string>
            {
                "r.\"Status\" = 1",
                "r.\"IsDeleted\" = false",
                "r.\"SearchVector\" @@ websearch_to_tsquery('simple', unaccent({0}))"
            };

            if (categoryId.HasValue)
            {
                conditions.Add($"r.\"CategoryId\" = {{{parameters.Count}}}");
                parameters.Add(categoryId.Value);
            }
            if (difficulty.HasValue)
            {
                conditions.Add($"r.\"Difficulty\" = {{{parameters.Count}}}");
                parameters.Add((short)difficulty.Value);
            }
            if (maxCookTime.HasValue)
            {
                conditions.Add($"r.\"CookTimeMinutes\" <= {{{parameters.Count}}}");
                parameters.Add(maxCookTime.Value);
            }
            if (minServings.HasValue)
            {
                conditions.Add($"r.\"Servings\" >= {{{parameters.Count}}}");
                parameters.Add(minServings.Value);
            }

            var whereSql = string.Join(" AND ", conditions);
            var orderBy = sort?.Trim() switch
            {
                "createdAt" => "r.\"CreatedAt\" ASC",
                "-createdAt" => "r.\"CreatedAt\" DESC",
                "title" => "r.\"Title\" ASC",
                "-title" => "r.\"Title\" DESC",
                "cookTime" => "r.\"CookTimeMinutes\" ASC",
                "-cookTime" => "r.\"CookTimeMinutes\" DESC",
                _ => "ts_rank(r.\"SearchVector\", websearch_to_tsquery('simple', unaccent({0}))) DESC, r.\"PublishedAt\" DESC"
            };

            var total = await db.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*)::int AS \"Value\" FROM \"Recipes\" r WHERE " + whereSql,
                    parameters.ToArray())
                .SingleAsync();

            var offsetIndex = parameters.Count;
            var offsetPlaceholder = "{" + offsetIndex + "}";
            var limitPlaceholder = "{" + (offsetIndex + 1) + "}";
            var sql = $"""
                SELECT r."Id" AS "Id",
                       r."Title" AS "Title",
                       r."Slug" AS "Slug",
                       r."Description" AS "Description",
                       (SELECT i."OriginalUrl"
                          FROM "RecipeImages" i
                         WHERE i."RecipeId" = r."Id"
                           AND i."IsPrimary" = true
                           AND i."IsDeleted" = false
                         ORDER BY i."OrderIndex"
                         LIMIT 1) AS "PrimaryImageUrl",
                       r."PrepTimeMinutes" AS "PrepTimeMinutes",
                       r."CookTimeMinutes" AS "CookTimeMinutes",
                       r."Servings" AS "Servings",
                       r."Difficulty" AS "Difficulty",
                       r."Status" AS "Status",
                       r."PublishedAt" AS "PublishedAt"
                  FROM "Recipes" r
                 WHERE {whereSql}
                 ORDER BY {orderBy}
                 OFFSET {offsetPlaceholder} LIMIT {limitPlaceholder}
                """;
            var rows = await db.Database
                .SqlQueryRaw<RecipeSearchRow>(sql,
                    parameters.Concat(new object[] { (page - 1) * pageSize, pageSize }).ToArray())
                .ToListAsync();

            var items = rows.Select(r => new FoodBlog.Application.Features.Recipes.RecipeSummaryDto(
                    r.Id, r.Title, r.Slug, r.Description ?? string.Empty, r.PrimaryImageUrl,
                    r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
                    ((Difficulty)r.Difficulty).ToString(), ((RecipeStatus)r.Status).ToString(),
                    r.PublishedAt))
                .ToList();

            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            return Results.Ok(new
            {
                items,
                totalCount = total,
                page,
                pageSize,
                totalPages,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            });
        });

        group.MapGet("/mine", async (ISender sender,
            int page = 1, int pageSize = 12, string? status = null) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            try
            {
                var result = await sender.Send(new GetMyRecipesQuery(page, pageSize, status));
                return Results.Ok(new
                {
                    items = result.Items,
                    totalCount = result.TotalCount,
                    page = result.Page,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasNextPage = result.HasNextPage,
                    hasPreviousPage = result.HasPreviousPage
                });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = ToErrors(ex) });
            }
        }).RequireAuthorization();

        // Dashboard cần chi tiết đầy đủ (steps/ingredients/images) của công thức
        // thuộc chính mình, kể cả khi đang là bản nháp.
        group.MapGet("/mine/{id:guid}", async (Guid id, ISender sender) =>
        {
            var recipe = await sender.Send(new GetMyRecipeByIdQuery(id));
            return recipe is null
                ? Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", id })
                : Results.Ok(recipe);
        });

        group.MapGet("/{slug}", async (string slug, ISender sender, FoodBlogDbContext db, HttpContext http) =>
        {
            RecipeDetailDto? recipe;
            try
            {
                recipe = await sender.Send(new GetRecipeBySlugQuery(slug));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = ToErrors(ex) });
            }
            if (recipe is null)
            {
                // FR-SEO-002: slug cũ sau khi đổi tiêu đề -> redirect 301 về slug hiện tại.
                var currentSlug = await db.Recipes.AsNoTracking()
                    .Where(r => r.PreviousSlug == slug && !r.IsDeleted)
                    .Select(r => r.Slug)
                    .FirstOrDefaultAsync();
                if (currentSlug is not null)
                    return Results.Redirect($"/api/v1/recipes/{Uri.EscapeDataString(currentSlug)}", permanent: true);

                return Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", slug });
            }
            if (recipe.Status != RecipeStatus.Published.ToString())
            {
                // Draft/Archived: chỉ chủ sở hữu hoặc Admin được xem (phục vụ trang edit).
                var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? http.User.FindFirstValue("sub");
                var canPreview = http.User.Identity?.IsAuthenticated == true
                    && (http.User.IsInRole("Admin") || userId == recipe.AuthorId);
                if (!canPreview)
                    return Results.Json(
                        new { code = "RECIPE_FORBIDDEN", title = "Bạn không có quyền xem công thức này." },
                        statusCode: 403);
            }
            return Results.Ok(recipe);
        });

        group.MapPost("/", async (CreateRecipeCommand req, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            try
            {
                var recipe = await sender.Send(req, ct);
                await store.EvictByTagAsync("recipes", ct);
                FoodBlogMetrics.RecipesCreated.Add(1);
                await audit.LogAsync(new AuditLogEntry("RECIPE_CREATED", "Recipe", recipe.Id.ToString(),
                    $"Tạo công thức \"{recipe.Title}\""), ct);
                return Results.Created($"/api/v1/recipes/{Uri.EscapeDataString(recipe.Slug)}", recipe);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = ToErrors(ex) });
            }
        }).RequireAuthorization("VerifiedAuthor").RequireRateLimiting("recipe-write");

        group.MapPut("/{id:guid}", async (Guid id, UpdateRecipeRequest req, ISender sender,
            IOutputCacheStore store, IAuditLogger audit, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateRecipeCommand(
                id, req.Title, req.Description, req.Instructions, req.CategoryId,
                req.PrepTimeMinutes, req.CookTimeMinutes, req.Servings, req.Difficulty, req.RowVersion,
                req.Nutrition), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_UPDATED", "Recipe", id.ToString(),
                $"Cập nhật công thức \"{result.Title}\""), ct);
            return Results.Ok(result);
        }).RequireAuthorization("VerifiedAuthor").RequireRateLimiting("recipe-write");

        group.MapPatch("/{id:guid}/publish", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            var result = await sender.Send(new PublishRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            FoodBlogMetrics.RecipesPublished.Add(1);
            await audit.LogAsync(new AuditLogEntry("RECIPE_PUBLISHED", "Recipe", id.ToString(),
                "Xuất bản công thức"), ct);
            return Results.Ok(result);
        }).RequireAuthorization("VerifiedAuthor").RequireRateLimiting("recipe-write");

        group.MapPatch("/{id:guid}/unpublish", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            var result = await sender.Send(new UnpublishRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_UNPUBLISHED", "Recipe", id.ToString(),
                "Gỡ xuất bản công thức"), ct);
            return Results.Ok(result);
        }).RequireAuthorization("VerifiedAuthor").RequireRateLimiting("recipe-write");

        group.MapPatch("/{id:guid}/archive", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            var result = await sender.Send(new ArchiveRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_ARCHIVED", "Recipe", id.ToString(),
                "Lưu trữ công thức"), ct);
            return Results.Ok(result);
        }).RequireAuthorization("VerifiedAuthor").RequireRateLimiting("recipe-write");

        group.MapPatch("/{id:guid}/unarchive", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            var result = await sender.Send(new UnarchiveRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_UNARCHIVED", "Recipe", id.ToString(),
                "Khôi phục công thức từ lưu trữ"), ct);
            return Results.Ok(result);
        }).RequireAuthorization("VerifiedAuthor").RequireRateLimiting("recipe-write");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            await sender.Send(new DeleteRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_DELETED", "Recipe", id.ToString(),
                "Xóa công thức"), ct);
            return Results.NoContent();
        }).RequireAuthorization("VerifiedAuthor").RequireRateLimiting("recipe-write");
    }

    private static Dictionary<string, string[]> ToErrors(FluentValidation.ValidationException ex) =>
        ex.Errors.GroupBy(f => ToCamelCase(f.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

    private static string ToCamelCase(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);
}

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

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/recipes")
            .WithApiVersionSet(apiVersionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Recipes")
            .RequireRateLimiting("general");

        group.MapGet("/", async (ISender sender,
            int page = 1, int pageSize = 12, Guid? categoryId = null,
            int? difficulty = null, int? maxCookTime = null, string sort = "-createdAt") =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var result = await sender.Send(new GetRecipesQuery(page, pageSize, categoryId, difficulty, maxCookTime, sort));
            return Results.Ok(new { items = result.Items, totalCount = result.TotalCount, page = result.Page, pageSize = result.PageSize });
        }).CacheOutput("RecipeList");

        group.MapGet("/search", async (FoodBlogDbContext db, string? q, int page = 1, int pageSize = 12) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.UnprocessableEntity(new { title = "Thiếu từ khóa tìm kiếm.", errors = new { q = new[] { "q là bắt buộc." } } });
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var baseQuery = db.Recipes.FromSql($"""
                SELECT * FROM "Recipes"
                WHERE "Status" = 1 AND "IsDeleted" = false
                  AND "SearchVector" @@ plainto_tsquery('english', unaccent({q}))
                """);
            var total = await baseQuery.CountAsync();
            var items = await baseQuery
                .OrderByDescending(r => r.PublishedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new FoodBlog.Application.Features.Recipes.RecipeSummaryDto(r.Id, r.Title, r.Slug, r.Description,
                    r.Images.Where(i => i.IsPrimary).Select(i => i.OriginalUrl).FirstOrDefault(),
                    r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
                    r.Difficulty.ToString(), r.Status.ToString(), r.PublishedAt))
                .ToListAsync();

            return Results.Ok(new { items, totalCount = total, page, pageSize });
        });

        group.MapGet("/mine", async (ISender sender,
            int page = 1, int pageSize = 12, string? status = null) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            try
            {
                var result = await sender.Send(new GetMyRecipesQuery(page, pageSize, status));
                return Results.Ok(new { items = result.Items, totalCount = result.TotalCount, page = result.Page, pageSize = result.PageSize });
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

        group.MapGet("/{slug}", async (string slug, ISender sender, HttpContext http) =>
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
                return Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", slug });
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
                await audit.LogAsync(new AuditLogEntry("RECIPE_CREATED", "Recipe", recipe.Id.ToString(),
                    $"Tạo công thức \"{recipe.Title}\""), ct);
                return Results.Created($"/api/v1/recipes/{Uri.EscapeDataString(recipe.Slug)}", recipe);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = ToErrors(ex) });
            }
        }).RequireAuthorization().RequireRateLimiting("recipe-write");

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
        }).RequireAuthorization().RequireRateLimiting("recipe-write");

        group.MapPatch("/{id:guid}/publish", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            var result = await sender.Send(new PublishRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_PUBLISHED", "Recipe", id.ToString(),
                "Xuất bản công thức"), ct);
            return Results.Ok(result);
        }).RequireAuthorization().RequireRateLimiting("recipe-write");

        group.MapPatch("/{id:guid}/unpublish", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            var result = await sender.Send(new UnpublishRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_UNPUBLISHED", "Recipe", id.ToString(),
                "Gỡ xuất bản công thức"), ct);
            return Results.Ok(result);
        }).RequireAuthorization().RequireRateLimiting("recipe-write");

        group.MapPatch("/{id:guid}/archive", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            var result = await sender.Send(new ArchiveRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_ARCHIVED", "Recipe", id.ToString(),
                "Lưu trữ công thức"), ct);
            return Results.Ok(result);
        }).RequireAuthorization().RequireRateLimiting("recipe-write");

        group.MapPatch("/{id:guid}/unarchive", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            var result = await sender.Send(new UnarchiveRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_UNARCHIVED", "Recipe", id.ToString(),
                "Khôi phục công thức từ lưu trữ"), ct);
            return Results.Ok(result);
        }).RequireAuthorization().RequireRateLimiting("recipe-write");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, IOutputCacheStore store,
            IAuditLogger audit, CancellationToken ct) =>
        {
            await sender.Send(new DeleteRecipeCommand(id), ct);
            await store.EvictByTagAsync("recipes", ct);
            await audit.LogAsync(new AuditLogEntry("RECIPE_DELETED", "Recipe", id.ToString(),
                "Xóa công thức"), ct);
            return Results.NoContent();
        }).RequireAuthorization().RequireRateLimiting("recipe-write");
    }

    private static Dictionary<string, string[]> ToErrors(FluentValidation.ValidationException ex) =>
        ex.Errors.GroupBy(f => ToCamelCase(f.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

    private static string ToCamelCase(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);
}

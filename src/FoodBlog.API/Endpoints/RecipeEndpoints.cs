using FoodBlog.Application.Features.Recipes;
using FoodBlog.Application.Features.Recipes.Commands;
using FoodBlog.Application.Features.Recipes.Queries;
using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using NpgsqlTypes;

namespace FoodBlog.API.Endpoints;

public record UpdateRecipeRequest(string Title, string Description, string Instructions, Guid CategoryId,
    int PrepTimeMinutes, int CookTimeMinutes, int Servings, int Difficulty, string RowVersion);

public static class RecipeEndpoints
{
    private static CancellationTokenSource _listCacheCts = new();

    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/recipes");

        group.MapGet("/", async (ISender sender, IMemoryCache cache,
            int page = 1, int pageSize = 12, Guid? categoryId = null,
            int? difficulty = null, int? maxCookTime = null, string sort = "-createdAt") =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var key = $"recipes:list:{page}:{pageSize}:{categoryId}:{difficulty}:{maxCookTime}:{sort}";

            if (cache.TryGetValue(key, out object? cached) && cached is not null)
                return Results.Ok(cached);

            var result = await sender.Send(new GetRecipesQuery(page, pageSize, categoryId, difficulty, maxCookTime, sort));
            var shaped = new { items = result.Items, totalCount = result.TotalCount, page = result.Page, pageSize = result.PageSize };
            cache.Set(key, shaped, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .AddExpirationToken(new CancellationChangeToken(_listCacheCts.Token)));
            return Results.Ok(shaped);
        });

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
                .Select(r => new RecipeSummaryDto(r.Id, r.Title, r.Slug, r.Description,
                    r.Images.Where(i => i.IsPrimary).Select(i => i.OriginalUrl).FirstOrDefault(),
                    r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
                    r.Difficulty.ToString(), r.PublishedAt))
                .ToListAsync();

            return Results.Ok(new { items, totalCount = total, page, pageSize });
        });

        group.MapGet("/{slug}", async (string slug, ISender sender) =>
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
                return Results.Json(new { code = "RECIPE_FORBIDDEN", title = "Bạn không có quyền xem công thức này." }, statusCode: 403);
            return Results.Ok(recipe);
        });

        group.MapPost("/", async (CreateRecipeCommand req, ISender sender) =>
        {
            try
            {
                var recipe = await sender.Send(req);
                EvictListCache();
                return Results.Created($"/api/v1/recipes/{Uri.EscapeDataString(recipe.Slug)}", recipe);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = ToErrors(ex) });
            }
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateRecipeRequest req, FoodBlogDbContext db) =>
        {
            var recipe = await db.Recipes
                .Include(r => r.Steps).Include(r => r.Ingredients).Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (recipe is null)
                return Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", id });

            byte[] clientVersion;
            try { clientVersion = Convert.FromBase64String(req.RowVersion); }
            catch { return Results.UnprocessableEntity(new { title = "RowVersion không hợp lệ." }); }
            if (recipe.RowVersion is null || !recipe.RowVersion.SequenceEqual(clientVersion))
                return Results.UnprocessableEntity(new { code = "RECIPE_CONCURRENCY_CONFLICT", title = "Dữ liệu đã bị thay đổi bởi người khác." });

            var errors = ValidateRecipe(req.Title, req.CategoryId, req.PrepTimeMinutes, req.CookTimeMinutes, req.Servings, req.Difficulty);
            if (errors.Count > 0)
                return Results.UnprocessableEntity(new { title = "Validation failed", errors });
            if (!await db.Categories.AnyAsync(c => c.Id == req.CategoryId))
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = new { categoryId = new[] { "Danh mục không tồn tại." } } });

            recipe.Title = req.Title.Trim();
            recipe.Description = req.Description;
            recipe.Instructions = req.Instructions;
            recipe.CategoryId = req.CategoryId;
            recipe.PrepTimeMinutes = req.PrepTimeMinutes;
            recipe.CookTimeMinutes = req.CookTimeMinutes;
            recipe.Servings = req.Servings;
            recipe.Difficulty = (Difficulty)req.Difficulty;
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Ok(RecipeMapper.ToDetail(recipe));
        });

        group.MapPatch("/{id:guid}/publish", async (Guid id, FoodBlogDbContext db) =>
        {
            var recipe = await db.Recipes.Include(r => r.Steps).FirstOrDefaultAsync(r => r.Id == id);
            if (recipe is null)
                return Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", id });
            if (recipe.Steps.Count == 0)
                return Results.UnprocessableEntity(new { code = "RECIPE_PUBLISH_INCOMPLETE", title = "Công thức cần ít nhất 1 bước thực hiện để xuất bản." });
            recipe.Status = RecipeStatus.Published;
            recipe.PublishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Ok(new { recipe.Id, status = recipe.Status.ToString() });
        });

        group.MapPatch("/{id:guid}/unpublish", async (Guid id, FoodBlogDbContext db) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe is null)
                return Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", id });
            recipe.Status = RecipeStatus.Draft;
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Ok(new { recipe.Id, status = recipe.Status.ToString() });
        });

        group.MapPatch("/{id:guid}/archive", async (Guid id, FoodBlogDbContext db) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe is null)
                return Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", id });
            recipe.Status = RecipeStatus.Archived;
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Ok(new { recipe.Id, status = recipe.Status.ToString() });
        });

        group.MapDelete("/{id:guid}", async (Guid id, FoodBlogDbContext db) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe is null)
                return Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", id });
            recipe.IsDeleted = true;
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.NoContent();
        });
    }

    public static void EvictListCache()
    {
        var old = Interlocked.Exchange(ref _listCacheCts, new CancellationTokenSource());
        old.Cancel();
        old.Dispose();
    }

    private static Dictionary<string, string[]> ToErrors(FluentValidation.ValidationException ex) =>
        ex.Errors.GroupBy(f => ToCamelCase(f.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

    private static string ToCamelCase(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);

    private static Dictionary<string, string[]> ValidateRecipe(string? title, Guid categoryId,
        int prep, int cook, int servings, int difficulty)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length is < 5 or > 200)
            errors["title"] = ["Tiêu đề phải từ 5 đến 200 ký tự."];
        if (categoryId == Guid.Empty)
            errors["categoryId"] = ["Danh mục không hợp lệ."];
        if (prep <= 0) errors["prepTimeMinutes"] = ["Thời gian chuẩn bị phải > 0."];
        if (cook < 0) errors["cookTimeMinutes"] = ["Thời gian nấu phải >= 0."];
        if (servings <= 0) errors["servings"] = ["Khẩu phần phải > 0."];
        if (difficulty is < 1 or > 4) errors["difficulty"] = ["Độ khó phải từ 1 đến 4."];
        return errors;
    }
}

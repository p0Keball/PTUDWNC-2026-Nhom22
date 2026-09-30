using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using NpgsqlTypes;

namespace FoodBlog.API.Endpoints;

public record NutritionDto(decimal? Calories, decimal? Protein, decimal? Carbohydrates,
    decimal? Fat, decimal? Fiber, decimal? Sodium);
public record StepDto(Guid Id, int StepNumber, string Title, string Description, int? TimerMinutes, string? ImageUrl);
public record IngredientDto(Guid Id, string Name, decimal? Quantity, string? Unit, string? Notes, int OrderIndex);
public record ImageDto(Guid Id, string OriginalUrl, string? MediumUrl, string? ThumbnailUrl,
    string? AltText, bool IsPrimary, int OrderIndex, string ThumbnailStatus);
public record RecipeDetailDto(Guid Id, string Title, string Slug, string Description, string Instructions,
    int PrepTimeMinutes, int CookTimeMinutes, int Servings, string Difficulty, string Status,
    Guid CategoryId, string AuthorId, DateTime? PublishedAt, string RowVersion,
    NutritionDto? Nutrition, List<StepDto> Steps, List<IngredientDto> Ingredients, List<ImageDto> Images);
public record CreateRecipeRequest(string Title, string Description, string Instructions, Guid CategoryId,
    int PrepTimeMinutes, int CookTimeMinutes, int Servings, int Difficulty);
public record UpdateRecipeRequest(string Title, string Description, string Instructions, Guid CategoryId,
    int PrepTimeMinutes, int CookTimeMinutes, int Servings, int Difficulty, string RowVersion);

public static class RecipeEndpoints
{
    private static CancellationTokenSource _listCacheCts = new();

    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/recipes");

        group.MapGet("/", async (FoodBlogDbContext db, IMemoryCache cache,
            int page = 1, int pageSize = 12, Guid? categoryId = null,
            int? difficulty = null, int? maxCookTime = null, string sort = "-createdAt") =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var key = $"recipes:list:{page}:{pageSize}:{categoryId}:{difficulty}:{maxCookTime}:{sort}";

            if (cache.TryGetValue(key, out object? cached) && cached is not null)
                return Results.Ok(cached);

            var query = db.Recipes.Where(r => r.Status == RecipeStatus.Published);
            if (categoryId.HasValue) query = query.Where(r => r.CategoryId == categoryId);
            if (difficulty.HasValue) query = query.Where(r => r.Difficulty == (Difficulty)difficulty);
            if (maxCookTime.HasValue) query = query.Where(r => r.CookTimeMinutes <= maxCookTime);
            query = sort switch
            {
                "createdAt" => query.OrderBy(r => r.CreatedAt),
                "title" => query.OrderBy(r => r.Title),
                "-title" => query.OrderByDescending(r => r.Title),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new RecipeSummaryDto(r.Id, r.Title, r.Slug, r.Description,
                    r.Images.Where(i => i.IsPrimary).Select(i => i.OriginalUrl).FirstOrDefault(),
                    r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
                    r.Difficulty.ToString(), r.PublishedAt))
                .ToListAsync();

            var result = new { items, totalCount = total, page, pageSize };
            cache.Set(key, result, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .AddExpirationToken(new CancellationChangeToken(_listCacheCts.Token)));
            return Results.Ok(result);
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

        group.MapGet("/{slug}", async (string slug, FoodBlogDbContext db) =>
        {
            var recipe = await db.Recipes
                .Include(r => r.Steps).Include(r => r.Ingredients).Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Slug == slug);
            if (recipe is null)
                return Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", slug });
            if (recipe.Status != RecipeStatus.Published)
                return Results.Json(new { code = "RECIPE_FORBIDDEN", title = "Bạn không có quyền xem công thức này." }, statusCode: 403);
            return Results.Ok(ToDetail(recipe));
        });

        group.MapPost("/", async (CreateRecipeRequest req, FoodBlogDbContext db) =>
        {
            var errors = ValidateRecipe(req.Title, req.CategoryId, req.PrepTimeMinutes, req.CookTimeMinutes, req.Servings, req.Difficulty);
            if (errors.Count > 0)
                return Results.UnprocessableEntity(new { title = "Validation failed", errors });
            if (!await db.Categories.AnyAsync(c => c.Id == req.CategoryId))
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = new { categoryId = new[] { "Danh mục không tồn tại." } } });

            var recipe = new Recipe
            {
                Title = req.Title.Trim(),
                Slug = await UniqueSlugAsync(db, SlugHelper.Generate(req.Title)),
                Description = req.Description,
                Instructions = req.Instructions,
                CategoryId = req.CategoryId,
                PrepTimeMinutes = req.PrepTimeMinutes,
                CookTimeMinutes = req.CookTimeMinutes,
                Servings = req.Servings,
                Difficulty = (Difficulty)req.Difficulty,
                Status = RecipeStatus.Draft,
                AuthorId = (await db.Users.Select(u => u.Id).FirstOrDefaultAsync()) ?? string.Empty
            };
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Created($"/api/v1/recipes/{Uri.EscapeDataString(recipe.Slug)}", ToDetail(recipe));
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
            return Results.Ok(ToDetail(recipe));
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

    private static async Task<string> UniqueSlugAsync(FoodBlogDbContext db, string baseSlug)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? Guid.NewGuid().ToString("N")[..8] : baseSlug;
        var candidate = slug;
        var counter = 2;
        while (await db.Recipes.AnyAsync(r => r.Slug == candidate))
            candidate = $"{slug}-{counter++}";
        return candidate;
    }

    public static RecipeDetailDto ToDetail(Recipe r) => new(
        r.Id, r.Title, r.Slug, r.Description, r.Instructions,
        r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
        r.Difficulty.ToString(), r.Status.ToString(),
        r.CategoryId, r.AuthorId, r.PublishedAt,
        Convert.ToBase64String(r.RowVersion ?? []),
        r.Nutrition is null ? null : new NutritionDto(r.Nutrition.Calories, r.Nutrition.Protein,
            r.Nutrition.Carbohydrates, r.Nutrition.Fat, r.Nutrition.Fiber, r.Nutrition.Sodium),
        r.Steps.OrderBy(s => s.StepNumber)
            .Select(s => new StepDto(s.Id, s.StepNumber, s.Title, s.Description, s.TimerMinutes, s.ImageUrl)).ToList(),
        r.Ingredients.OrderBy(i => i.OrderIndex)
            .Select(i => new IngredientDto(i.Id, i.Name, i.Quantity, i.Unit, i.Notes, i.OrderIndex)).ToList(),
        r.Images.OrderBy(i => i.OrderIndex)
            .Select(i => new ImageDto(i.Id, i.OriginalUrl, i.MediumUrl, i.ThumbnailUrl, i.AltText, i.IsPrimary, i.OrderIndex,
                i.ThumbnailUrl is null && i.MediumUrl is null ? "pending" : "ready")).ToList());
}
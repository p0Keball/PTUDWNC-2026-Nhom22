using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FoodBlog.API.Endpoints;

public record CategoryDto(Guid Id, string Name, string Slug, string? Description, string? ImageUrl, int OrderIndex, int RecipeCount);
public record RecipeSummaryDto(Guid Id, string Title, string Slug, string Description);
public record CreateCategoryRequest(string Name, string? Description, string? ImageUrl);
public record UpdateCategoryRequest(string Name, string? Description, string? ImageUrl);

public static class CategoryEndpoints
{
    private const string CacheKey = "categories:all";

    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/categories");

        group.MapGet("/", async (FoodBlogDbContext db, IMemoryCache cache) =>
        {
            if (cache.TryGetValue(CacheKey, out List<CategoryDto>? cached) && cached is not null)
                return Results.Ok(cached);

            var items = await db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto(
                    c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.OrderIndex,
                    c.Recipes.Count(r => r.Status == RecipeStatus.Published)))
                .ToListAsync();

            cache.Set(CacheKey, items, TimeSpan.FromMinutes(60));
            return Results.Ok(items);
        });

        group.MapGet("/{slug}", async (string slug, FoodBlogDbContext db, int page = 1, int pageSize = 12) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
            if (category is null)
                return Results.NotFound(new { title = "Category not found", slug });

            var query = db.Recipes.Where(r => r.CategoryId == category.Id && r.Status == RecipeStatus.Published);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RecipeSummaryDto(r.Id, r.Title, r.Slug, r.Description))
                .ToListAsync();

            return Results.Ok(new
            {
                category = new CategoryDto(category.Id, category.Name, category.Slug,
                    category.Description, category.ImageUrl, category.OrderIndex, total),
                recipes = new { items, totalCount = total, page, pageSize }
            });
        });

        group.MapPost("/", async (CreateCategoryRequest req, FoodBlogDbContext db, IMemoryCache cache) =>
        {
            var errors = ValidateName(req.Name);
            if (errors.Count > 0)
                return Results.UnprocessableEntity(new { title = "Validation failed", errors });

            if (await db.Categories.AnyAsync(c => c.Name == req.Name.Trim()))
                return Results.Conflict(new { code = "CATEGORY_NAME_EXISTS", title = "Tên danh mục đã tồn tại." });

            var slug = await UniqueSlugAsync(db, SlugHelper.Generate(req.Name));
            var category = new Category
            {
                Name = req.Name.Trim(),
                Slug = slug,
                Description = req.Description,
                ImageUrl = req.ImageUrl
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            cache.Remove(CacheKey);

            return Results.Created($"/api/v1/categories/{category.Slug}",
                new CategoryDto(category.Id, category.Name, category.Slug,
                    category.Description, category.ImageUrl, category.OrderIndex, 0));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest req, FoodBlogDbContext db, IMemoryCache cache) =>
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
                return Results.NotFound(new { title = "Category not found", id });

            var errors = ValidateName(req.Name);
            if (errors.Count > 0)
                return Results.UnprocessableEntity(new { title = "Validation failed", errors });

            var nameTaken = await db.Categories.AnyAsync(c => c.Id != id && c.Name == req.Name.Trim());
            if (nameTaken)
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = new { name = new[] { "Tên danh mục đã tồn tại." } } });

            category.Name = req.Name.Trim();
            category.Description = req.Description;
            category.ImageUrl = req.ImageUrl;
            await db.SaveChangesAsync();
            cache.Remove(CacheKey);

            var count = await db.Recipes.CountAsync(r => r.CategoryId == id && r.Status == RecipeStatus.Published);
            return Results.Ok(new CategoryDto(category.Id, category.Name, category.Slug,
                category.Description, category.ImageUrl, category.OrderIndex, count));
        });

        group.MapDelete("/{id:guid}", async (Guid id, FoodBlogDbContext db, IMemoryCache cache) =>
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
                return Results.NotFound(new { title = "Category not found", id });

            var recipeCount = await db.Recipes.CountAsync(r => r.CategoryId == id);
            if (recipeCount > 0)
                return Results.Conflict(new { title = $"Danh mục còn chứa {recipeCount} công thức.", recipeCount });

            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            cache.Remove(CacheKey);
            return Results.NoContent();
        });
    }

    private static Dictionary<string, string[]> ValidateName(string? name)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length is < 2 or > 50)
            errors["name"] = ["Tên danh mục phải từ 2 đến 50 ký tự."];
        return errors;
    }

    private static async Task<string> UniqueSlugAsync(FoodBlogDbContext db, string baseSlug)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? Guid.NewGuid().ToString("N")[..8] : baseSlug;
        var candidate = slug;
        var counter = 2;
        while (await db.Categories.AnyAsync(c => c.Slug == candidate))
            candidate = $"{slug}-{counter++}";
        return candidate;
    }
}

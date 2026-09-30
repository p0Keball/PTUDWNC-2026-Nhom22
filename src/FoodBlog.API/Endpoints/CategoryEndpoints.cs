using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;

namespace FoodBlog.API.Endpoints;

public record CategoryDto(Guid Id, string Name, string Slug, string? Description, string? ImageUrl, int OrderIndex, int RecipeCount);
public record RecipeSummaryDto(Guid Id, string Title, string Slug, string Description,
    string? PrimaryImageUrl, int PrepTimeMinutes, int CookTimeMinutes, int Servings,
    string Difficulty, DateTime? PublishedAt);
public record CreateCategoryRequest(string Name, string? Description, string? ImageUrl);
public record UpdateCategoryRequest(string Name, string? Description, string? ImageUrl);

public static class CategoryEndpoints
{
    private const string CacheKey = "categories:all";

    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/categories");

        group.MapGet("/", async (IUnitOfWork uow, IMemoryCache cache) =>
        {
            if (cache.TryGetValue(CacheKey, out List<CategoryDto>? cached) && cached is not null)
                return Results.Ok(cached);

            var categories = await uow.Categories.ListOrderedWithRecipesAsync();
            var items = categories
                .Select(c => new CategoryDto(
                    c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.OrderIndex,
                    c.Recipes.Count(r => r.Status == RecipeStatus.Published)))
                .ToList();

            cache.Set(CacheKey, items, TimeSpan.FromMinutes(60));
            return Results.Ok(items);
        });

        group.MapGet("/{slug}", async (string slug, IUnitOfWork uow, int page = 1, int pageSize = 12) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var category = await uow.Categories.GetBySlugAsync(slug)
                ?? throw new NotFoundException("CATEGORY_NOT_FOUND", "Category not found");

            var (recipes, total) = await uow.Categories.GetPublishedRecipesAsync(category.Id, page, pageSize);
            var items = recipes
                .Select(r => new RecipeSummaryDto(r.Id, r.Title, r.Slug, r.Description,
                    r.Images.Where(i => i.IsPrimary).Select(i => i.OriginalUrl).FirstOrDefault(),
                    r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
                    r.Difficulty.ToString(), r.PublishedAt))
                .ToList();

            return Results.Ok(new
            {
                category = new CategoryDto(category.Id, category.Name, category.Slug,
                    category.Description, category.ImageUrl, category.OrderIndex, total),
                recipes = new { items, totalCount = total, page, pageSize }
            });
        });

        group.MapPost("/", async (CreateCategoryRequest req, IUnitOfWork uow, IMemoryCache cache) =>
        {
            ThrowIfInvalidName(req.Name);

            if (await uow.Categories.ExistsByNameAsync(req.Name.Trim()))
                throw new ConflictException("CATEGORY_NAME_EXISTS", "Tên danh mục đã tồn tại.");

            var slug = await UniqueSlugAsync(uow, SlugHelper.Generate(req.Name));
            var category = new Category
            {
                Name = req.Name.Trim(),
                Slug = slug,
                Description = req.Description,
                ImageUrl = req.ImageUrl
            };
            await uow.Categories.AddAsync(category);
            await uow.SaveChangesAsync();
            cache.Remove(CacheKey);

            return Results.Created($"/api/v1/categories/{Uri.EscapeDataString(category.Slug)}",
                new CategoryDto(category.Id, category.Name, category.Slug,
                    category.Description, category.ImageUrl, category.OrderIndex, 0));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest req, IUnitOfWork uow, IMemoryCache cache) =>
        {
            var category = await uow.Categories.GetByIdAsync(id)
                ?? throw new NotFoundException("CATEGORY_NOT_FOUND", "Category not found");

            ThrowIfInvalidName(req.Name);

            if (await uow.Categories.ExistsByNameAsync(req.Name.Trim(), id))
                throw new Domain.Exceptions.ValidationException(
                    "Validation failed",
                    new Dictionary<string, string[]> { ["name"] = ["Tên danh mục đã tồn tại."] });

            category.Name = req.Name.Trim();
            category.Description = req.Description;
            category.ImageUrl = req.ImageUrl;
            await uow.SaveChangesAsync();
            cache.Remove(CacheKey);

            var count = await uow.Categories.CountPublishedRecipesAsync(id);
            return Results.Ok(new CategoryDto(category.Id, category.Name, category.Slug,
                category.Description, category.ImageUrl, category.OrderIndex, count));
        });

        group.MapDelete("/{id:guid}", async (Guid id, IUnitOfWork uow, IMemoryCache cache) =>
        {
            var category = await uow.Categories.GetByIdAsync(id)
                ?? throw new NotFoundException("CATEGORY_NOT_FOUND", "Category not found");

            var recipeCount = await uow.Categories.CountAllRecipesAsync(id);
            if (recipeCount > 0)
                throw new ConflictException("CATEGORY_DELETE_HAS_RECIPES",
                    $"Danh mục còn chứa {recipeCount} công thức.");

            uow.Categories.Remove(category);
            await uow.SaveChangesAsync();
            cache.Remove(CacheKey);
            return Results.NoContent();
        });
    }

    private static void ThrowIfInvalidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length is < 2 or > 50)
            throw new Domain.Exceptions.ValidationException(
                "Validation failed",
                new Dictionary<string, string[]> { ["name"] = ["Tên danh mục phải từ 2 đến 50 ký tự."] });
    }

    private static async Task<string> UniqueSlugAsync(IUnitOfWork uow, string baseSlug)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? Guid.NewGuid().ToString("N")[..8] : baseSlug;
        var candidate = slug;
        var counter = 2;
        while (await uow.Categories.ExistsBySlugAsync(candidate))
            candidate = $"{slug}-{counter++}";
        return candidate;
    }
}

using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FoodBlog.Tests.Infrastructure;

internal static class TestData
{
    public const string AuthorId = "test-author-1";

    /// <summary>Tạo user + category + recipe Published; trả về RecipeId.</summary>
    public static async Task<Guid> SeedRecipeAsync(IServiceProvider services, Guid? recipeId = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();

        if (!await db.Users.AnyAsync(u => u.Id == AuthorId))
        {
            var user = ApplicationUser.Create("Test Author", "author@foodblog.test", "author@foodblog.test");
            user.Id = AuthorId;
            db.Users.Add(user);
        }

        var category = new Category
        {
            Name = "Món Việt",
            Slug = $"mon-viet-{Guid.NewGuid():N}",
            OrderIndex = 1,
        };
        db.Categories.Add(category);

        var recipe = new Recipe
        {
            Id = recipeId ?? Guid.NewGuid(),
            Title = "Phở bò",
            Slug = $"pho-bo-{Guid.NewGuid():N}",
            Description = "Công thức phở bò dùng cho integration test.",
            Instructions = "Nấu nước dùng, trụng bánh phở, xếp thịt rồi chan nước.",
            PrepTimeMinutes = 30,
            CookTimeMinutes = 120,
            Servings = 4,
            Difficulty = Difficulty.Medium,
            Status = RecipeStatus.Published,
            PublishedAt = DateTime.UtcNow,
            CategoryId = category.Id,
            AuthorId = AuthorId,
        };
        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();

        return recipe.Id;
    }

    /// <summary>Mở scope mới để đọc dữ liệu (tránh change tracker của request trước).</summary>
    public static async Task<T> QueryAsync<T>(
        IServiceProvider services,
        Func<FoodBlogDbContext, Task<T>> query)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
        return await query(db);
    }

    public static async Task<T> QueryAsync<T>(
        IServiceProvider services,
        Func<FoodBlogDbContext, IQueryable<T>> query)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
        return await query(db).FirstAsync();
    }
}

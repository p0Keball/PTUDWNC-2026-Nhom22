using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bogus;
using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FoodBlog.Infrastructure.Seed;

public static class FoodBlogSeeder
{
    private static readonly string[] CategoryNames =
    [
        "Món chính", "Món tráng miệng", "Món chay", "Món nướng", "Món hấp",
        "Món xào", "Món canh", "Món lẩu", "Bánh ngọt", "Đồ uống",
        "Món ăn sáng", "Món khai vị", "Món hải sản", "Món gà", "Món bò",
        "Món heo", "Món chay Âu", "Salad", "Món Tết", "Ăn vặt"
    ];

    private static T CreateEntity<T>(params (string name, object? value)[] properties) where T : class
    {
        var instance = (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;
        var type = typeof(T);

        foreach (var (name, value) in properties)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            prop?.SetValue(instance, value);
        }
        return instance;
    }

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Khởi tạo Roles nếu chưa có
        foreach (var role in new[] { "Admin", "Author" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2. Khởi tạo Admin nếu chưa có
        var admin = await userManager.FindByEmailAsync("admin@foodblog.local");
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@foodblog.local",
                DisplayName = "Quản trị viên",
                IsActive = true
            };
            await userManager.CreateAsync(admin, "Foodblog123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // 3. Khởi tạo Authors nếu chưa đủ
        var existingAuthors = (await userManager.GetUsersInRoleAsync("Author")).ToList();
        if (existingAuthors.Count < 5)
        {
            var authorFaker = new Faker<ApplicationUser>()
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.Email, (f, u) => $"{u.UserName}@foodblog.local")
                .RuleFor(u => u.DisplayName, f => f.Name.FullName())
                .RuleFor(u => u.IsActive, true);

            for (var i = existingAuthors.Count; i < 5; i++)
            {
                var author = authorFaker.Generate();
                await userManager.CreateAsync(author, "Foodblog123!");
                await userManager.AddToRoleAsync(author, "Author");
                existingAuthors.Add(author);
            }
        }

        var allUsers = new List<ApplicationUser> { admin };
        allUsers.AddRange(existingAuthors);

        // 4. Khởi tạo Categories
        var usedSlugs = new HashSet<string>(await db.Categories.Select(c => c.Slug).ToListAsync());
        var categories = await db.Categories.ToListAsync();
        if (categories.Count == 0)
        {
            categories = CategoryNames
                .Select((name, i) => CreateEntity<Category>(
                    ("Id", Guid.NewGuid()),
                    ("Name", name),
                    ("Slug", UniqueSlug(SlugHelper.Generate(name), usedSlugs)),
                    ("Description", $"Các công thức thuộc nhóm {name}."),
                    ("OrderIndex", i)
                ))
                .ToList();

            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();
        }

        var categoryMap = categories.ToDictionary(c => c.Name, c => c);

        // 5. Kiểm tra và dọn dẹp các công thức cũ/dữ liệu ảo (như 'Phở bò 1', 'Bánh chuối 4'...)
        var dummyRecipes = await db.Recipes
            .Where(r => r.Description.Contains("dễ làm tại nhà") ||
                        r.Title.EndsWith(" 1") || r.Title.EndsWith(" 2") ||
                        r.Title.EndsWith(" 3") || r.Title.EndsWith(" 4") ||
                        r.Title.EndsWith(" 5"))
            .ToListAsync();

        if (dummyRecipes.Count > 0)
        {
            db.Recipes.RemoveRange(dummyRecipes);
            await db.SaveChangesAsync();
        }

        // 6. Seed các công thức thực tế đã kiểm chứng
        var existingTitles = new HashSet<string>(await db.Recipes.Select(r => r.Title).ToListAsync());
        var existingRecipeSlugs = new HashSet<string>(await db.Recipes.Select(r => r.Slug).ToListAsync());
        var verifiedItems = VerifiedRecipesData.GetRecipes();
        var newRecipes = new List<Recipe>();

        for (var i = 0; i < verifiedItems.Count; i++)
        {
            var item = verifiedItems[i];
            if (existingTitles.Contains(item.Title))
                continue;

            var category = categoryMap.TryGetValue(item.CategoryName, out var cat) ? cat : categories[0];
            var author = allUsers[i % allUsers.Count];
            var recipeId = Guid.NewGuid();

            var nutrition = CreateEntity<RecipeNutrition>(
                ("Calories", (decimal?)item.Calories),
                ("Protein", (decimal?)item.Protein),
                ("Carbohydrates", (decimal?)item.Carbohydrates),
                ("Fat", (decimal?)item.Fat),
                ("Fiber", (decimal?)item.Fiber),
                ("Sodium", (decimal?)item.Sodium)
            );

            var slug = UniqueSlug(SlugHelper.Generate(item.Title), existingRecipeSlugs);
            var recipe = CreateEntity<Recipe>(
                ("Id", recipeId),
                ("Title", item.Title),
                ("Slug", slug),
                ("Description", item.Description),
                ("Instructions", item.Instructions),
                ("PrepTimeMinutes", item.PrepTimeMinutes),
                ("CookTimeMinutes", item.CookTimeMinutes),
                ("Servings", item.Servings),
                ("Difficulty", item.Difficulty),
                ("Status", item.Status),
                ("CategoryId", category.Id),
                ("AuthorId", author.Id),
                ("PublishedAt", item.Status == RecipeStatus.Published ? (DateTime?)DateTime.UtcNow.AddDays(-(i % 30)) : null),
                ("Nutrition", nutrition)
            );

            for (var k = 0; k < item.Ingredients.Count; k++)
            {
                var ing = item.Ingredients[k];
                recipe.Ingredients.Add(CreateEntity<RecipeIngredient>(
                    ("Name", ing.Name),
                    ("Quantity", ing.Quantity),
                    ("Unit", ing.Unit),
                    ("Notes", ing.Notes),
                    ("OrderIndex", k)
                ));
            }

            for (var s = 0; s < item.Steps.Count; s++)
            {
                var step = item.Steps[s];
                recipe.Steps.Add(CreateEntity<RecipeStep>(
                    ("StepNumber", s + 1),
                    ("Title", step.Title),
                    ("Description", step.Description),
                    ("TimerMinutes", step.TimerMinutes)
                ));
            }

            for (var m = 0; m < item.Images.Count; m++)
            {
                var img = item.Images[m];
                recipe.Images.Add(CreateEntity<RecipeImage>(
                    ("OriginalUrl", img.Url),
                    ("AltText", img.AltText),
                    ("IsPrimary", m == 0),
                    ("OrderIndex", m)
                ));
            }

            newRecipes.Add(recipe);
        }

        if (newRecipes.Count > 0)
        {
            db.Recipes.AddRange(newRecipes);
            await db.SaveChangesAsync();
        }
    }

    private static string UniqueSlug(string baseSlug, HashSet<string> used)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? Guid.NewGuid().ToString("N")[..8] : baseSlug;
        var candidate = slug;
        var counter = 2;
        while (!used.Add(candidate))
            candidate = $"{slug}-{counter++}";
        return candidate;
    }
}

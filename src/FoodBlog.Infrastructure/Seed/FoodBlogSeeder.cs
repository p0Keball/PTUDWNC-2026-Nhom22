using Bogus;
using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
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

    private static readonly string[] DishCores =
    [
        "Phở bò", "Bánh mì", "Cơm tấm", "Bún chả", "Gỏi cuốn",
        "Bánh xèo", "Mì Quảng", "Hủ tiếu", "Chả giò", "Canh chua",
        "Lẩu thái", "Bò kho", "Gà nướng", "Cá kho", "Tôm rim",
        "Bánh flan", "Chè đậu", "Sinh tố bơ", "Cà phê trứng", "Bánh chuối"
    ];

    private static readonly string[] IngredientPool =
    [
        "Thịt bò", "Thịt heo", "Gà ta", "Tôm sú", "Cá basa",
        "Trứng gà", "Đậu hũ", "Nấm rơm", "Rau muống", "Cải thìa",
        "Hành lá", "Tỏi", "Ớt hiểm", "Gừng", "Sả",
        "Nước mắm", "Đường", "Muối", "Tiêu", "Dầu ăn",
        "Nước dừa", "Sữa tươi", "Bột mì", "Gạo tẻ", "Bún tươi"
    ];

    private static readonly string[] Units = ["g", "ml", "thìa canh", "thìa cà phê", "củ", "nhánh", "chén", "quả"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (db.Categories.Any())
            return;

        Randomizer.Seed = new Random(20260922);
        var usedSlugs = new HashSet<string>();

        foreach (var role in new[] { "Admin", "Author" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@foodblog.local",
            DisplayName = "Quản trị viên",
            IsActive = true
        };
        await userManager.CreateAsync(admin, "Foodblog123!");
        await userManager.AddToRoleAsync(admin, "Admin");

        var authors = new List<ApplicationUser>();
        var authorFaker = new Faker<ApplicationUser>()
            .RuleFor(u => u.UserName, f => f.Internet.UserName())
            .RuleFor(u => u.Email, (f, u) => $"{u.UserName}@foodblog.local")
            .RuleFor(u => u.DisplayName, f => f.Name.FullName())
            .RuleFor(u => u.IsActive, true);
        for (var i = 0; i < 5; i++)
        {
            var author = authorFaker.Generate();
            await userManager.CreateAsync(author, "Foodblog123!");
            await userManager.AddToRoleAsync(author, "Author");
            authors.Add(author);
        }

        var allUsers = new List<ApplicationUser> { admin };
        allUsers.AddRange(authors);

        var categories = CategoryNames
            .Select((name, i) => new Category
            {
                Name = name,
                Slug = UniqueSlug(SlugHelper.Generate(name), usedSlugs),
                Description = $"Các công thức thuộc nhóm {name}.",
                OrderIndex = i
            })
            .ToList();
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var recipes = new List<Recipe>();
        for (var i = 0; i < 100; i++)
        {
            var core = DishCores[i % DishCores.Length];
            var title = $"{core} {(i / DishCores.Length) + 1}";
            var status = i < 80 ? RecipeStatus.Published
                : i < 95 ? RecipeStatus.Draft : RecipeStatus.Archived;
            var author = allUsers[i % allUsers.Count];
            var category = categories[i % categories.Count];
            var recipeId = Guid.NewGuid();

            var recipe = new Recipe
            {
                Id = recipeId,
                Title = title,
                Slug = UniqueSlug(SlugHelper.Generate(title), usedSlugs),
                Description = $"Công thức {title} thơm ngon, dễ làm tại nhà.",
                Instructions = $"Hướng dẫn tổng quát cách chế biến {title}.",
                PrepTimeMinutes = 10 + (i % 40),
                CookTimeMinutes = 10 + (i % 50),
                Servings = 2 + (i % 4),
                Difficulty = (Difficulty)((i % 4) + 1),
                Status = status,
                CategoryId = category.Id,
                AuthorId = author.Id,
                PublishedAt = status == RecipeStatus.Published
                    ? DateTime.UtcNow.AddDays(-(i % 60))
                    : null,
                Nutrition = new RecipeNutrition
                {
                    Calories = 150 + (i % 400),
                    Protein = 5 + (i % 30),
                    Carbohydrates = 10 + (i % 60),
                    Fat = 3 + (i % 25),
                    Fiber = 1 + (i % 8),
                    Sodium = 100 + (i % 900)
                }
            };

            var ingredientCount = 10 + (i % 3);
            for (var k = 0; k < ingredientCount; k++)
            {
                recipe.Ingredients.Add(new RecipeIngredient
                {
                    Name = IngredientPool[(i + k) % IngredientPool.Length],
                    Quantity = 50 + ((i * 7 + k * 13) % 500),
                    Unit = Units[(i + k) % Units.Length],
                    OrderIndex = k
                });
            }

            var stepCount = 5 + (i % 4);
            for (var s = 0; s < stepCount; s++)
            {
                recipe.Steps.Add(new RecipeStep
                {
                    StepNumber = s + 1,
                    Title = $"Bước {s + 1}: Sơ chế và chuẩn bị",
                    Description = $"Mô tả chi tiết bước {s + 1} của món {title}.",
                    TimerMinutes = s % 2 == 0 ? 5 + ((i + s) % 20) : null
                });
            }

            var imageCount = 1 + (i % 3);
            for (var m = 0; m < imageCount; m++)
            {
                recipe.Images.Add(new RecipeImage
                {
                    OriginalUrl = $"http://localhost:9000/culinary-blog/recipes/{recipeId}/{Guid.NewGuid():N}.jpg",
                    AltText = $"Ảnh minh họa {title} ({m + 1})",
                    IsPrimary = m == 0,
                    OrderIndex = m
                });
            }

            recipes.Add(recipe);

            if (recipes.Count % 25 == 0)
            {
                db.Recipes.AddRange(recipes);
                await db.SaveChangesAsync();
                recipes.Clear();
            }
        }

        if (recipes.Count > 0)
        {
            db.Recipes.AddRange(recipes);
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

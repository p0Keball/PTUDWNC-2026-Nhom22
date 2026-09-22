using Bogus;
using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Infrastructure.Persistence.Seed;

/// <summary>
/// Seed dữ liệu mẫu Tuần 1 (SRS §2.6.1: Bogus, 2 user / 5 category / 3 recipe).
/// Idempotent: bỏ qua khi DB đã có category. Randomizer.Seed cố định
/// để mọi máy dev ra cùng một bộ dữ liệu.
/// </summary>
public static class FoodBlogSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        if (await db.Categories.AnyAsync(ct))
            return;

        Randomizer.Seed = new Random(22);
        var faker = new Faker("en");

        // 1. Users (TV1)
        var admin = ApplicationUser.Create("admin@foodblog.local", "admin", "Quản Trị Viên");
        var author = ApplicationUser.Create("bep.truong@foodblog.local", "beptruong", "Bếp Trưởng");
        db.Users.AddRange(admin, author);

        // 2. Categories (TV2)
        var categories = new[]
        {
            Category.Create("Món chính", "Các món ăn chính trong bữa cơm gia đình Việt", orderIndex: 1),
            Category.Create("Món tráng miệng", "Chè, bánh ngọt và món ăn vặt sau bữa chính", orderIndex: 2),
            Category.Create("Món chay", "Món ăn thanh đạm từ rau củ, đậu hũ và nấm", orderIndex: 3),
            Category.Create("Đồ uống", "Sinh tố, nước ép và trà thảo mộc giải khát", orderIndex: 4),
            Category.Create("Bữa sáng", "Món ăn nhanh gọn, đủ chất cho buổi sáng", orderIndex: 5),
        };
        db.Categories.AddRange(categories);

        var monChinh = categories[0];
        var trangMieng = categories[1];
        var monChay = categories[2];

        // 3. Recipes (TV3) + Steps/Ingredients/Images (TV4)
        var phoBo = Recipe.Create(
            "Phở bò Hà Nội",
            "Tô phở bò chuẩn vị Hà Nội với nước dùng trong, thơm mùi quế hồi.",
            monChinh.Id, author.Id,
            prepTimeMinutes: 30, cookTimeMinutes: 180, servings: 4,
            difficulty: DifficultyLevel.Medium,
            instructions: "Hầm xương bò 3 tiếng, trụng bánh phở, chan nước dùng nóng.");
        SeedPhoBo(phoBo, faker);
        phoBo.SetNutrition(RecipeNutrition.Create(calories: 450, protein: 32, carbohydrates: 48, fat: 14));

        var cheBaMau = Recipe.Create(
            "Chè ba màu",
            "Ly chè ba màu mát lạnh với đậu đỏ, đậu xanh và thạch rau câu.",
            trangMieng.Id, author.Id,
            prepTimeMinutes: 20, cookTimeMinutes: 40, servings: 6,
            difficulty: DifficultyLevel.Easy,
            instructions: "Nấu từng loại đậu, làm thạch, xếp lớp và chan nước cốt dừa.");
        SeedCheBaMau(cheBaMau, faker);

        var saladChay = Recipe.Create(
            "Salad rau củ chay",
            "Đĩa salad giòn tươi với sốt mè rang, phù hợp người ăn chay.",
            monChay.Id, admin.Id,
            prepTimeMinutes: 15, cookTimeMinutes: 0, servings: 2,
            difficulty: DifficultyLevel.Easy,
            instructions: "Rửa sạch rau củ, cắt vừa ăn, trộn sốt mè rang trước khi dùng.");
        SeedSaladChay(saladChay, faker);
        // Giữ salad ở Draft để demo phân quyền Guest/Author (FR-RCP-001).

        db.Recipes.AddRange(phoBo, cheBaMau, saladChay);
        await db.SaveChangesAsync(ct);
    }

    private static void SeedPhoBo(Recipe recipe, Faker faker)
    {
        AddSteps(recipe,
            ("Hầm xương", "Rửa sạch xương bò, chần sơ rồi hầm với hành gừng nướng.", 180),
            ("Nêm nước dùng", "Nêm muối, đường phèn, thêm quế, hồi, thảo quả rang thơm.", 15),
            ("Trụng bánh phở", "Trụng bánh phở và thịt bò tái trong nước sôi.", 3),
            ("Hoàn thiện", "Xếp bánh phở, thịt, hành lá rồi chan nước dùng thật nóng.", null));

        AddIngredients(recipe,
            ("Xương bò", 1, "kg", "Chọn xương ống nhiều tủy"),
            ("Bánh phở tươi", 800, "g", null),
            ("Thịt bò thăn", 400, "g", "Thái lát mỏng"),
            ("Quế, hồi, thảo quả", 1, "gói", "Rang thơm trước khi cho vào"),
            ("Hành lá, rau thơm", 100, "g", null));

        AddPrimaryImage(recipe, "pho-bo-ha-noi.jpg", "Tô phở bò Hà Nội nghi ngút khói");
        recipe.Publish();
    }

    private static void SeedCheBaMau(Recipe recipe, Faker faker)
    {
        AddSteps(recipe,
            ("Nấu đậu đỏ", "Ngâm đậu đỏ 4 tiếng rồi nấu mềm với đường.", 40),
            ("Nấu đậu xanh", "Hấp chín đậu xanh, đánh nhuyễn với nước cốt dừa.", 25),
            ("Làm thạch", "Nấu rau câu với lá dứa, để đông rồi cắt hạt lựu.", 20),
            ("Xếp ly", "Xếp 3 lớp đậu + thạch, thêm đá bào và nước cốt dừa.", null));

        AddIngredients(recipe,
            ("Đậu đỏ", 200, "g", null),
            ("Đậu xanh cà vỏ", 200, "g", null),
            ("Bột rau câu", 10, "g", null),
            ("Nước cốt dừa", 400, "ml", null),
            ("Đường", 250, "g", "Gia giảm theo khẩu vị"));

        AddPrimaryImage(recipe, "che-ba-mau.jpg", "Ly chè ba màu mát lạnh");
        recipe.Publish();
    }

    private static void SeedSaladChay(Recipe recipe, Faker faker)
    {
        AddSteps(recipe,
            ("Sơ chế", "Rửa sạch xà lách, cà chua bi, dưa leo; để ráo nước.", 10),
            ("Pha sốt", "Trộn mè rang, dầu ô liu, giấm táo và mật ong.", 5),
            ("Trộn salad", "Trộn đều rau củ với sốt ngay trước khi ăn.", 2));

        AddIngredients(recipe,
            ("Xà lách", 200, "g", null),
            ("Cà chua bi", 150, "g", null),
            ("Dưa leo", 1, "quả", null),
            ("Mè rang", 20, "g", null));

        AddPrimaryImage(recipe, "salad-chay.jpg", "Đĩa salad rau củ chay");
    }

    private static void AddSteps(Recipe recipe, params (string Title, string Desc, int? Timer)[] steps)
    {
        var faker = new Faker();
        var number = 1;
        foreach (var (title, desc, timer) in steps)
        {
            var step = RecipeStep.Create(
                recipe.Id, number++, title, desc,
                timerMinutes: timer ?? faker.Random.Int(2, 10));
            recipe.Steps.Add(step);
        }
    }

    private static void AddIngredients(Recipe recipe, params (string Name, decimal Qty, string Unit, string? Notes)[] items)
    {
        var order = 0;
        foreach (var (name, qty, unit, notes) in items)
        {
            recipe.Ingredients.Add(RecipeIngredient.Create(
                recipe.Id, name, qty, unit, notes, order++));
        }
    }

    private static void AddPrimaryImage(Recipe recipe, string fileName, string altText)
    {
        // Placeholder theo quy ước path FR-RCP-008 (recipes/{recipeId}/{file}).
        // Ảnh thật do flow upload lên MinIO thay thế sau.
        recipe.Images.Add(RecipeImage.Create(
            recipe.Id,
            $"http://localhost:9000/culinary-blog/recipes/{recipe.Slug}/{fileName}",
            altText, isPrimary: true));
    }
}

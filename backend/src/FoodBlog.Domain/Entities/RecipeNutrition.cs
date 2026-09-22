namespace FoodBlog.Domain.Entities;

/// <summary>
/// Thông tin dinh dưỡng (SRS §7.2.1).
/// QUYẾT ĐỊNH: Owned Entity — EF Core nhúng thẳng vào bảng Recipes
/// với tiền tố cột Nutrition_* (không có bảng riêng, không có Id).
/// </summary>
public class RecipeNutrition
{
    public decimal? Calories { get; private set; }
    public decimal? Protein { get; private set; }
    public decimal? Carbohydrates { get; private set; }
    public decimal? Fat { get; private set; }
    public decimal? Fiber { get; private set; }
    public decimal? Sodium { get; private set; }

    protected RecipeNutrition() { }

    public static RecipeNutrition Create(
        decimal? calories = null,
        decimal? protein = null,
        decimal? carbohydrates = null,
        decimal? fat = null,
        decimal? fiber = null,
        decimal? sodium = null)
    {
        return new RecipeNutrition
        {
            Calories = calories,
            Protein = protein,
            Carbohydrates = carbohydrates,
            Fat = fat,
            Fiber = fiber,
            Sodium = sodium,
        };
    }
}

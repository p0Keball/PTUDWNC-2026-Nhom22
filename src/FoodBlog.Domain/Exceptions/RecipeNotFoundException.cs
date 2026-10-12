namespace FoodBlog.Domain.Exceptions;

public sealed class RecipeNotFoundException : DomainException
{
    public RecipeNotFoundException(Guid recipeId)
        : base("Không tìm thấy công thức.", "RECIPE_NOT_FOUND")
    {
        RecipeId = recipeId;
    }

    public Guid RecipeId { get; }
}

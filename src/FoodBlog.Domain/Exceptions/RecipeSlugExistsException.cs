namespace FoodBlog.Domain.Exceptions;

public sealed class RecipeSlugExistsException : DomainException
{
    public RecipeSlugExistsException(string slug)
        : base($"Slug '{slug}' đã tồn tại.", "RECIPE_SLUG_EXISTS")
    {
        Slug = slug;
    }

    public string Slug { get; }
}

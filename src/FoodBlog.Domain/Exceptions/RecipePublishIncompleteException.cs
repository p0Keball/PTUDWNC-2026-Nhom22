namespace FoodBlog.Domain.Exceptions;

public sealed class RecipePublishIncompleteException : DomainException
{
    public RecipePublishIncompleteException()
        : base("Công thức cần ít nhất 1 bước thực hiện để xuất bản.", "RECIPE_PUBLISH_INCOMPLETE")
    {
    }
}

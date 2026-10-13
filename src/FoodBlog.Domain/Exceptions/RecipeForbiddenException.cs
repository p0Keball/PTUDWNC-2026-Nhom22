namespace FoodBlog.Domain.Exceptions;

public class RecipeForbiddenException : DomainException
{
    public RecipeForbiddenException()
        : base("Bạn không có quyền sửa công thức này.", "RECIPE_FORBIDDEN")
    {
    }
}

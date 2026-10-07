namespace FoodBlog.Domain.Exceptions;

public class RecipeUnauthorizedException : DomainException
{
    public RecipeUnauthorizedException()
        : base("Bạn cần đăng nhập để thực hiện thao tác này.")
    {
    }
}

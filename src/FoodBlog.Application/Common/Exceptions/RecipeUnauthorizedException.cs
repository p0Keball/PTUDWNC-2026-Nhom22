namespace FoodBlog.Application.Common.Exceptions;

public sealed class RecipeUnauthorizedException : Exception
{
    public RecipeUnauthorizedException()
        : base("Bạn cần đăng nhập để thực hiện thao tác này.")
    {
    }
}

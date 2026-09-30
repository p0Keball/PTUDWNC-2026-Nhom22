namespace FoodBlog.Application.Common.Exceptions;

public sealed class RecipeForbiddenException : Exception
{
    public RecipeForbiddenException()
        : base("Bạn không có quyền sửa công thức này.")
    {
    }
}

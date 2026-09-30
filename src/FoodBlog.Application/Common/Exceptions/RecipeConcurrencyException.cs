namespace FoodBlog.Application.Common.Exceptions;

public sealed class RecipeConcurrencyException : Exception
{
    public RecipeConcurrencyException()
        : base("Dữ liệu đã bị thay đổi bởi người khác.")
    {
    }
}

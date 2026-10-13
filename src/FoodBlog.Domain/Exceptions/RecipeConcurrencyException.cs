namespace FoodBlog.Domain.Exceptions;

public class RecipeConcurrencyException : DomainException
{
    public RecipeConcurrencyException()
        : base("Dữ liệu đã bị thay đổi bởi người khác.", "RECIPE_CONCURRENCY_CONFLICT")
    {
    }
}

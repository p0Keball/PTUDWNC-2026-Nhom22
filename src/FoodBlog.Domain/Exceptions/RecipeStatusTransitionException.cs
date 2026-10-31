namespace FoodBlog.Domain.Exceptions;

/// <summary>
/// Chuyển trạng thái công thức không hợp lệ, ví dụ Publish một công thức đã lưu trữ.
/// </summary>
public sealed class RecipeStatusTransitionException : DomainException
{
    public RecipeStatusTransitionException(string message)
        : base(message, "RECIPE_INVALID_STATUS_TRANSITION")
    {
    }
}

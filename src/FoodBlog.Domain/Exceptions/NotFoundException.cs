namespace FoodBlog.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message, string? errorCode = null)
        : base(message, errorCode)
    {
    }
}
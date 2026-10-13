namespace FoodBlog.Domain.Exceptions;

public sealed class ConflictException : DomainException
{
    public ConflictException(string message, string? errorCode = null)
        : base(message, errorCode)
    {
    }
}
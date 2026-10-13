namespace FoodBlog.Domain.Exceptions;

public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Unauthorized.", string? errorCode = null)
        : base(message, errorCode)
    {
    }
}
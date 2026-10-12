namespace FoodBlog.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public string? ErrorCode { get; }
}

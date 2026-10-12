namespace FoodBlog.Domain.Exceptions;

public sealed class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(
        IReadOnlyDictionary<string, string[]> errors,
        string? errorCode = "VALIDATION_ERROR")
        : base("One or more validation errors occurred.", errorCode)
    {
        Errors = errors;
    }
}
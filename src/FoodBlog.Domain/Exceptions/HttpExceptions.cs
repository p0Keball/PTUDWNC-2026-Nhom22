using Microsoft.AspNetCore.Http;

namespace FoodBlog.Domain.Exceptions;

public sealed class NotFoundException(string code, string message)
    : DomainException(code, message, StatusCodes.Status404NotFound);

public sealed class ConflictException(string code, string message)
    : DomainException(code, message, StatusCodes.Status409Conflict);

public sealed class ForbiddenException(string code, string message)
    : DomainException(code, message, StatusCodes.Status403Forbidden);

public sealed class ValidationException(string message, IDictionary<string, string[]> errors)
    : DomainException("VALIDATION_ERROR", message, StatusCodes.Status422UnprocessableEntity)
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}

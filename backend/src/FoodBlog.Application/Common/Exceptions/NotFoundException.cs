namespace FoodBlog.Application.Common.Exceptions;

/// <summary>
/// Ném khi entity không tồn tại — Global Exception Middleware chuyển thành 404 RFC 7807.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} với khóa '{key}' không tồn tại.")
    {
    }
}

namespace FoodBlog.Domain.Exceptions;

/// <summary>
/// Exception gốc cho mọi lỗi nghiệp vụ thuộc Domain Layer.
/// Infrastructure/Application catch và chuyển thành RFC 7807 problem details.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message, string code = "DOMAIN_ERROR")
        : base(message)
    {
        Code = code;
    }

    public DomainException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}

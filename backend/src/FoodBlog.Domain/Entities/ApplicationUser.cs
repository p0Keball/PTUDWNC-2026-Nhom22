namespace FoodBlog.Domain.Entities;

/// <summary>
/// Người dùng hệ thống (SRS §7.7).
/// QUYẾT ĐỊNH: class độc lập với Id kiểu string (KHÔNG kế thừa BaseEntity)
/// để khi Chương 2引入 ASP.NET Core Identity, class này chỉ cần đổi thành
/// "ApplicationUser : IdentityUser" mà KHÔNG phải sửa FK Recipe.AuthorId
/// (varchar 450) ở bất kỳ đâu. Dùng DisplayName thống nhất (FAILURE F-11),
/// không dùng fullName.
/// </summary>
public class ApplicationUser
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool EmailConfirmed { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public ICollection<Recipe> Recipes { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    protected ApplicationUser() { }

    public static ApplicationUser Create(string email, string userName, string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
        ArgumentException.ThrowIfNullOrWhiteSpace(userName, nameof(userName));
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));

        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email.Trim().ToLowerInvariant(),
            UserName = userName.Trim(),
            DisplayName = displayName.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateProfile(string displayName, string? avatarUrl, string? bio)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));

        DisplayName = displayName.Trim();
        AvatarUrl = avatarUrl?.Trim();
        Bio = bio?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

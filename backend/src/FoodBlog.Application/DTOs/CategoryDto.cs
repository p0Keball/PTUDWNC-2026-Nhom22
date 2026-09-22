namespace FoodBlog.Application.DTOs;

/// <summary>
/// DTO trả về cho Category — không expose Entity ra ngoài (Mapster map theo convention).
/// </summary>
public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int OrderIndex,
    DateTime CreatedAt
);

using FoodBlog.Application.DTOs;
using MediatR;

namespace FoodBlog.Application.Features.Categories.Commands.CreateCategory;

/// <summary>
/// Command tạo danh mục mới (CQRS §1.2.4). Record bất biến.
/// </summary>
public record CreateCategoryCommand(
    string Name,
    string? Description,
    string? ImageUrl = null,
    int OrderIndex = 0
) : IRequest<CategoryDto>;

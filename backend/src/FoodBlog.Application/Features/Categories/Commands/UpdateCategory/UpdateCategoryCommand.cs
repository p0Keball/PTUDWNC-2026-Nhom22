using FoodBlog.Application.DTOs;
using MediatR;

namespace FoodBlog.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl = null,
    int? OrderIndex = null
) : IRequest<CategoryDto>;

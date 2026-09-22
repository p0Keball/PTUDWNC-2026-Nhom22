using FoodBlog.Application.DTOs;
using MediatR;

namespace FoodBlog.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;

using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.DTOs;
using FoodBlog.Domain.Entities;
using Mapster;
using MediatR;

namespace FoodBlog.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // 1. Tạo entity qua Domain factory — đảm bảo business rules
        var category = Category.Create(request.Name, request.Description, request.ImageUrl, request.OrderIndex);

        // 2. Thêm vào DbContext (chưa ghi DB)
        _context.Categories.Add(category);

        // 3. Ghi DB (SQL INSERT)
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Map sang DTO — Presentation không nhận raw Entity
        return category.Adapt<CategoryDto>();
    }
}

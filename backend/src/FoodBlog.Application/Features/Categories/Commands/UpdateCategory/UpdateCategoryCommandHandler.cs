using FoodBlog.Application.Common.Exceptions;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.DTOs;
using FoodBlog.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        category.Update(request.Name, request.Description, request.ImageUrl, request.OrderIndex);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Adapt<CategoryDto>();
    }
}

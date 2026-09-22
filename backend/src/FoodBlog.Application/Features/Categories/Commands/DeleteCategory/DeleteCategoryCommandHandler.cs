using FoodBlog.Application.Common.Exceptions;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        // FR-CAT-005: chặn xóa khi còn recipe
        var hasRecipes = await _context.Recipes
            .AnyAsync(r => r.CategoryId == request.Id, cancellationToken);
        if (hasRecipes)
            throw new InvalidOperationException("Không thể xóa danh mục đang chứa công thức.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

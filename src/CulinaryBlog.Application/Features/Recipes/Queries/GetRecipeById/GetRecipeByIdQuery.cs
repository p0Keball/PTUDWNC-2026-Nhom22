using CulinaryBlog.Application.DTOs;
using CulinaryBlog.Application.Contracts.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeById;

public record GetRecipeByIdQuery(Guid Id) : IRequest<RecipeDetailDto?>;

public class GetRecipeByIdQueryHandler : IRequestHandler<GetRecipeByIdQuery, RecipeDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetRecipeByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<RecipeDetailDto?> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Recipes
            .AsNoTracking()
            .Include(r => r.Category)
            .Where(r => r.Id == request.Id)
            .Select(r => new RecipeDetailDto(
                r.Id,
                r.Title,
                r.Description,
                r.Instructions,
                r.PrepTimeMinutes,
                r.CookTimeMinutes,
                r.Servings,
                r.Difficulty,
                r.CategoryId,
                r.Category.Name,
                r.AuthorId,
                r.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

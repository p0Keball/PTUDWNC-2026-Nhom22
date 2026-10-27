using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Recipes.Queries;

/// <summary>
/// Lấy chi tiết công thức của chính tác giả (kể cả bản nháp/lưu trữ).
/// Dùng cho trang chỉnh sửa trong dashboard — không phụ thuộc trang public.
/// </summary>
public record GetMyRecipeByIdQuery(Guid Id) : IRequest<RecipeDetailDto?>;

public sealed class GetMyRecipeByIdQueryHandler(IFoodBlogDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMyRecipeByIdQuery, RecipeDetailDto?>
{
    public async Task<RecipeDetailDto?> Handle(GetMyRecipeByIdQuery req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new RecipeUnauthorizedException();

        var recipe = await db.Recipes
            .AsNoTracking()
            .Include(r => r.Steps)
            .Include(r => r.Ingredients)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(
                r => r.Id == req.Id && r.AuthorId == currentUser.UserId,
                ct);

        return recipe is null ? null : RecipeMapper.ToDetail(recipe);
    }
}

using FluentValidation;
using FoodBlog.Application.Common.Exceptions;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Features.Recipes;
using FoodBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Recipes.Commands;

public record UpdateRecipeCommand(
    Guid Id,
    string Title,
    string Description,
    string Instructions,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    int Difficulty,
    string RowVersion) : IRequest<RecipeDetailDto>;

public sealed class UpdateRecipeCommandValidator : AbstractValidator<UpdateRecipeCommand>
{
    public UpdateRecipeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id không hợp lệ.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MinimumLength(5).WithMessage("Tiêu đề phải từ 5 đến 200 ký tự.")
            .MaximumLength(200).WithMessage("Tiêu đề phải từ 5 đến 200 ký tự.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Mô tả không được để trống.");
        RuleFor(x => x.Instructions).NotEmpty().WithMessage("Hướng dẫn không được để trống.");
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Danh mục không hợp lệ.");
        RuleFor(x => x.PrepTimeMinutes).GreaterThan(0).WithMessage("Thời gian chuẩn bị phải > 0.");
        RuleFor(x => x.CookTimeMinutes).GreaterThanOrEqualTo(0).WithMessage("Thời gian nấu phải >= 0.");
        RuleFor(x => x.Servings).GreaterThan(0).WithMessage("Khẩu phần phải > 0.");
        RuleFor(x => x.Difficulty).InclusiveBetween(1, 4).WithMessage("Độ khó phải từ 1 đến 4.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("RowVersion là bắt buộc.");
    }
}

public sealed class UpdateRecipeCommandHandler(IFoodBlogDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<UpdateRecipeCommand, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(UpdateRecipeCommand req, CancellationToken ct)
    {
        var recipe = await db.Recipes
            .Include(r => r.Steps)
            .Include(r => r.Ingredients)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == req.Id, ct);

        if (recipe is null)
            throw new KeyNotFoundException($"Recipe {req.Id} not found.");

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new RecipeUnauthorizedException();

        if (!currentUser.IsAdmin && recipe.AuthorId != currentUser.UserId)
            throw new RecipeForbiddenException();

        byte[] clientVersion;
        try
        {
            clientVersion = Convert.FromBase64String(req.RowVersion);
        }
        catch
        {
            throw new ValidationException(
                [new FluentValidation.Results.ValidationFailure("RowVersion", "RowVersion không hợp lệ.")]);
        }

        if (recipe.RowVersion is null || !recipe.RowVersion.SequenceEqual(clientVersion))
            throw new RecipeConcurrencyException();

        if (!await db.Categories.AnyAsync(c => c.Id == req.CategoryId, ct))
            throw new ValidationException(
                [new FluentValidation.Results.ValidationFailure("CategoryId", "Danh mục không tồn tại.")]);

        recipe.Title = req.Title.Trim();
        recipe.Description = req.Description;
        recipe.Instructions = req.Instructions;
        recipe.CategoryId = req.CategoryId;
        recipe.PrepTimeMinutes = req.PrepTimeMinutes;
        recipe.CookTimeMinutes = req.CookTimeMinutes;
        recipe.Servings = req.Servings;
        recipe.Difficulty = (Difficulty)req.Difficulty;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new RecipeConcurrencyException();
        }

        return RecipeMapper.ToDetail(recipe);
    }
}

using FluentValidation;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Features.Recipes;
using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Recipes.Commands;

public record CreateRecipeCommand(
    string Title,
    string Description,
    string Instructions,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    int Difficulty) : IRequest<RecipeDetailDto>;

public sealed class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator()
    {
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
    }
}

public sealed class CreateRecipeCommandHandler(IFoodBlogDbContext db) : IRequestHandler<CreateRecipeCommand, RecipeDetailDto>
{
    public async Task<RecipeDetailDto> Handle(CreateRecipeCommand req, CancellationToken ct)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == req.CategoryId, ct))
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("CategoryId", "Danh mục không tồn tại.")]);

        var recipe = new Recipe
        {
            Title = req.Title.Trim(),
            Slug = await UniqueSlugAsync(SlugHelper.Generate(req.Title), ct),
            Description = req.Description,
            Instructions = req.Instructions,
            CategoryId = req.CategoryId,
            PrepTimeMinutes = req.PrepTimeMinutes,
            CookTimeMinutes = req.CookTimeMinutes,
            Servings = req.Servings,
            Difficulty = (Difficulty)req.Difficulty,
            Status = RecipeStatus.Draft,
            AuthorId = await db.Users.Select(u => u.Id).FirstOrDefaultAsync(ct) ?? string.Empty
        };

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync(ct);
        return RecipeMapper.ToDetail(recipe);
    }

    private async Task<string> UniqueSlugAsync(string baseSlug, CancellationToken ct)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? Guid.NewGuid().ToString("N")[..8] : baseSlug;
        var candidate = slug;
        var counter = 2;
        while (await db.Recipes.AnyAsync(r => r.Slug == candidate, ct))
            candidate = $"{slug}-{counter++}";
        return candidate;
    }
}

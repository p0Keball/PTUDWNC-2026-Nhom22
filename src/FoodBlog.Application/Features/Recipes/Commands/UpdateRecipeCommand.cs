using FluentValidation;
using FoodBlog.Application.Features.Recipes;
using MediatR;

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

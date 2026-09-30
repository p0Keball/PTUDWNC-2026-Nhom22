using FluentValidation;

namespace FoodBlog.API.Endpoints;

/// <summary>
/// Validators cho child-data (Step/Ingredient) dùng FluentValidation,
/// tuân thủ CONS-008 (không validation thủ công trong handler).
/// </summary>
public sealed class CreateStepValidator : AbstractValidator<CreateStepRequest>
{
    public CreateStepValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Tiêu đề bước không được rỗng.")
            .MinimumLength(3).WithMessage("Tiêu đề bước phải từ 3 đến 200 ký tự.")
            .MaximumLength(200).WithMessage("Tiêu đề bước phải từ 3 đến 200 ký tự.");
        RuleFor(x => x.Description).MaximumLength(5000).WithMessage("Mô tả tối đa 5000 ký tự.");
        RuleFor(x => x.TimerMinutes).GreaterThanOrEqualTo(0).When(x => x.TimerMinutes.HasValue)
            .WithMessage("Timer phải >= 0.");
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl is not null)
            .WithMessage("ImageUrl tối đa 500 ký tự.");
    }
}

public sealed class UpdateStepValidator : AbstractValidator<UpdateStepRequest>
{
    public UpdateStepValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Tiêu đề bước không được rỗng.")
            .MinimumLength(3).WithMessage("Tiêu đề bước phải từ 3 đến 200 ký tự.")
            .MaximumLength(200).WithMessage("Tiêu đề bước phải từ 3 đến 200 ký tự.");
        RuleFor(x => x.Description).MaximumLength(5000).WithMessage("Mô tả tối đa 5000 ký tự.");
        RuleFor(x => x.TimerMinutes).GreaterThanOrEqualTo(0).When(x => x.TimerMinutes.HasValue)
            .WithMessage("Timer phải >= 0.");
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl is not null)
            .WithMessage("ImageUrl tối đa 500 ký tự.");
    }
}

public sealed class CreateIngredientValidator : AbstractValidator<CreateIngredientRequest>
{
    public CreateIngredientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên nguyên liệu không được rỗng.")
            .MinimumLength(2).WithMessage("Tên nguyên liệu phải từ 2 đến 200 ký tự.")
            .MaximumLength(200).WithMessage("Tên nguyên liệu phải từ 2 đến 200 ký tự.");
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue)
            .WithMessage("Số lượng phải > 0.");
        RuleFor(x => x.Unit).MaximumLength(50).When(x => x.Unit is not null)
            .WithMessage("Đơn vị tối đa 50 ký tự.");
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null)
            .WithMessage("Ghi chú tối đa 500 ký tự.");
    }
}

public sealed class UpdateIngredientValidator : AbstractValidator<UpdateIngredientRequest>
{
    public UpdateIngredientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên nguyên liệu không được rỗng.")
            .MinimumLength(2).WithMessage("Tên nguyên liệu phải từ 2 đến 200 ký tự.")
            .MaximumLength(200).WithMessage("Tên nguyên liệu phải từ 2 đến 200 ký tự.");
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue)
            .WithMessage("Số lượng phải > 0.");
        RuleFor(x => x.Unit).MaximumLength(50).When(x => x.Unit is not null)
            .WithMessage("Đơn vị tối đa 50 ký tự.");
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null)
            .WithMessage("Ghi chú tối đa 500 ký tự.");
    }
}

public static class ChildValidationResult
{
    public static IResult? ToUnprocessable<T>(IValidator<T> validator, T instance)
    {
        var result = validator.Validate(instance);
        if (result.IsValid)
            return null;
        var errors = result.Errors.GroupBy(f => ToCamelCase(f.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
        return Results.UnprocessableEntity(new { title = "Validation failed", errors });
    }

    private static string ToCamelCase(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);
}

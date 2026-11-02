using FluentValidation;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.Features.Recipes;
using FoodBlog.Domain.Enums;
using FoodBlog.Domain.Exceptions;
using MediatR;

namespace FoodBlog.Application.Features.Recipes.Commands;

/// <summary>
/// Khôi phục công thức đã lưu trữ (Archived -> Draft), hoàn thiện luồng
/// Archive / Unarchive của FR-RCP-006.
/// </summary>
public record RestoreRecipeCommand(Guid Id) : IRequest<RecipeStatusDto>;

public sealed class RestoreRecipeCommandValidator : AbstractValidator<RestoreRecipeCommand>
{
    public RestoreRecipeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id không hợp lệ.");
    }
}

public sealed class RestoreRecipeCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<RestoreRecipeCommand, RecipeStatusDto>
{
    public async Task<RecipeStatusDto> Handle(RestoreRecipeCommand req, CancellationToken ct)
    {
        var recipe = await uow.Recipes.GetForUpdateAsync(req.Id, ct);
        if (recipe is null)
            throw new RecipeNotFoundException(req.Id);

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new RecipeUnauthorizedException();

        if (!currentUser.IsAdmin && recipe.AuthorId != currentUser.UserId)
            throw new RecipeForbiddenException();

        if (recipe.Status != RecipeStatus.Archived)
            throw new FoodBlog.Domain.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                ["status"] = ["Chỉ có thể khôi phục công thức đang ở trạng thái lưu trữ."]
            });

        recipe.Status = RecipeStatus.Draft;
        recipe.PublishedAt = null;

        await uow.SaveChangesAsync(ct);
        return new RecipeStatusDto(recipe.Id, recipe.Status.ToString());
    }
}

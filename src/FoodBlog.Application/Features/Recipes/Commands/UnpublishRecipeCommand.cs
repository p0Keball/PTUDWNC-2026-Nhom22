using FluentValidation;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.Features.Recipes;
using FoodBlog.Domain.Enums;
using FoodBlog.Domain.Exceptions;
using MediatR;

namespace FoodBlog.Application.Features.Recipes.Commands;

public record UnpublishRecipeCommand(Guid Id) : IRequest<RecipeStatusDto>;

public sealed class UnpublishRecipeCommandValidator : AbstractValidator<UnpublishRecipeCommand>
{
    public UnpublishRecipeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id không hợp lệ.");
    }
}

public sealed class UnpublishRecipeCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UnpublishRecipeCommand, RecipeStatusDto>
{
    public async Task<RecipeStatusDto> Handle(UnpublishRecipeCommand req, CancellationToken ct)
    {
        var recipe = await uow.Recipes.GetByIdAsync(req.Id, ct);
        if (recipe is null)
            throw new RecipeNotFoundException(req.Id);

        recipe.Status = RecipeStatus.Draft;

        await uow.SaveChangesAsync(ct);
        return new RecipeStatusDto(recipe.Id, recipe.Status.ToString());
    }
}

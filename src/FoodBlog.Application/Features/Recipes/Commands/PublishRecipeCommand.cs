using FluentValidation;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.Features.Recipes;
using FoodBlog.Domain.Enums;
using FoodBlog.Domain.Exceptions;
using MediatR;

namespace FoodBlog.Application.Features.Recipes.Commands;

public record PublishRecipeCommand(Guid Id) : IRequest<RecipeStatusDto>;

public sealed class PublishRecipeCommandValidator : AbstractValidator<PublishRecipeCommand>
{
    public PublishRecipeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id không hợp lệ.");
    }
}

public sealed class PublishRecipeCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<PublishRecipeCommand, RecipeStatusDto>
{
    public async Task<RecipeStatusDto> Handle(PublishRecipeCommand req, CancellationToken ct)
    {
        var recipe = await uow.Recipes.GetForUpdateAsync(req.Id, ct);
        if (recipe is null)
            throw new RecipeNotFoundException(req.Id);

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new RecipeUnauthorizedException();

        if (!currentUser.IsAdmin && recipe.AuthorId != currentUser.UserId)
            throw new RecipeForbiddenException();

        if (recipe.Steps.Count == 0)
            throw new RecipePublishIncompleteException();

        recipe.Status = RecipeStatus.Published;
        recipe.PublishedAt = DateTime.UtcNow;

        await uow.SaveChangesAsync(ct);
        return new RecipeStatusDto(recipe.Id, recipe.Status.ToString());
    }
}

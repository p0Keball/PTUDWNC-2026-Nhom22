using FluentValidation;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.Features.Recipes;
using FoodBlog.Domain.Enums;
using FoodBlog.Domain.Exceptions;
using MediatR;

namespace FoodBlog.Application.Features.Recipes.Commands;

public record ArchiveRecipeCommand(Guid Id) : IRequest<RecipeStatusDto>;

public sealed class ArchiveRecipeCommandValidator : AbstractValidator<ArchiveRecipeCommand>
{
    public ArchiveRecipeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id không hợp lệ.");
    }
}

public sealed class ArchiveRecipeCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<ArchiveRecipeCommand, RecipeStatusDto>
{
    public async Task<RecipeStatusDto> Handle(ArchiveRecipeCommand req, CancellationToken ct)
    {
        var recipe = await uow.Recipes.GetByIdAsync(req.Id, ct);
        if (recipe is null)
            throw new RecipeNotFoundException(req.Id);

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new RecipeUnauthorizedException();

        if (!currentUser.IsAdmin && recipe.AuthorId != currentUser.UserId)
            throw new RecipeForbiddenException();

        recipe.Status = RecipeStatus.Archived;

        await uow.SaveChangesAsync(ct);
        return new RecipeStatusDto(recipe.Id, recipe.Status.ToString());
    }
}

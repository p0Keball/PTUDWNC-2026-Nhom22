using FluentValidation;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.Features.Recipes;
using FoodBlog.Domain.Enums;
using FoodBlog.Domain.Exceptions;
using MediatR;

namespace FoodBlog.Application.Features.Recipes.Commands;

public record UnarchiveRecipeCommand(Guid Id) : IRequest<RecipeStatusDto>;

public sealed class UnarchiveRecipeCommandValidator : AbstractValidator<UnarchiveRecipeCommand>
{
    public UnarchiveRecipeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id không hợp lệ.");
    }
}

/// <summary>
/// FR-RCP-006 — Khôi phục công thức đã lưu trữ về trạng thái nháp.
/// </summary>
public sealed class UnarchiveRecipeCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<UnarchiveRecipeCommand, RecipeStatusDto>
{
    public async Task<RecipeStatusDto> Handle(UnarchiveRecipeCommand req, CancellationToken ct)
    {
        var recipe = await uow.Recipes.GetByIdAsync(req.Id, ct);
        if (recipe is null)
            throw new RecipeNotFoundException(req.Id);

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new RecipeUnauthorizedException();

        if (!currentUser.IsAdmin && recipe.AuthorId != currentUser.UserId)
            throw new RecipeForbiddenException();

        if (recipe.Status == RecipeStatus.Published)
            throw new RecipeStatusTransitionException(
                "Công thức đang xuất bản. Hãy gỡ xuất bản trước khi khôi phục từ lưu trữ.");

        recipe.Status = RecipeStatus.Draft;

        await uow.SaveChangesAsync(ct);
        return new RecipeStatusDto(recipe.Id, recipe.Status.ToString());
    }
}

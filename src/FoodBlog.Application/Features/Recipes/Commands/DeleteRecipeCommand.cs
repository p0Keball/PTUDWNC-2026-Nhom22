using FluentValidation;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Domain.Exceptions;
using MediatR;

namespace FoodBlog.Application.Features.Recipes.Commands;

public record DeleteRecipeCommand(Guid Id) : IRequest;

public sealed class DeleteRecipeCommandValidator : AbstractValidator<DeleteRecipeCommand>
{
    public DeleteRecipeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id không hợp lệ.");
    }
}

public sealed class DeleteRecipeCommandHandler(IUnitOfWork uow)
    : IRequestHandler<DeleteRecipeCommand>
{
    public async Task Handle(DeleteRecipeCommand req, CancellationToken ct)
    {
        var recipe = await uow.Recipes.GetByIdAsync(req.Id, ct);
        if (recipe is null)
            throw new RecipeNotFoundException(req.Id);

        recipe.IsDeleted = true;

        await uow.SaveChangesAsync(ct);
    }
}

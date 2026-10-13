using FluentValidation;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.Interfaces;
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

public sealed class DeleteRecipeCommandHandler(
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IImageJobQueue jobs)
    : IRequestHandler<DeleteRecipeCommand>
{
    public async Task Handle(DeleteRecipeCommand req, CancellationToken ct)
    {
        // GetForUpdateAsync đã Include Images để enqueue MinIO cleanup.
        var recipe = await uow.Recipes.GetForUpdateAsync(req.Id, ct);
        if (recipe is null)
            throw new RecipeNotFoundException(req.Id);

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new RecipeUnauthorizedException();

        if (!currentUser.IsAdmin && recipe.AuthorId != currentUser.UserId)
            throw new RecipeForbiddenException();

        var imageUrls = recipe.Images.Select(i => i.OriginalUrl).ToList();

        recipe.IsDeleted = true;

        await uow.SaveChangesAsync(ct);

        // MinIO cleanup: xóa đệ quy ảnh gốc + biến thể dưới nền (fire-and-forget).
        jobs.EnqueueCleanup(imageUrls);
    }
}

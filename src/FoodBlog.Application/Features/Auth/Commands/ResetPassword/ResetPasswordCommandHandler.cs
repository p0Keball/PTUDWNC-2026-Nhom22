using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FoodBlog.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Request.Email.Trim().ToLowerInvariant());
        if (user is null || !user.IsActive)
            throw new NotFoundException("Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.");

        var resetResult = await userManager.ResetPasswordAsync(user, request.Request.Token, request.Request.NewPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }
    }
}

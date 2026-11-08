using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FoodBlog.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new UnauthorizedAccessException("Không xác định được người dùng.");

        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null || !user.IsActive)
            throw new NotFoundException("Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.");

        var isCurrentPasswordCorrect = await userManager.CheckPasswordAsync(user, request.Request.CurrentPassword);
        if (!isCurrentPasswordCorrect)
            throw new UnauthorizedException("Mật khẩu hiện tại không chính xác.");

        var changeResult = await userManager.ChangePasswordAsync(user, request.Request.CurrentPassword, request.Request.NewPassword);
        if (!changeResult.Succeeded)
        {
            var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }
    }
}

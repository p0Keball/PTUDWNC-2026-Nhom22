using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Application.Interfaces;
using FoodBlog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FoodBlog.Application.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Request.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);

        // Luôn trả về thành công (không tiết lộ email có tồn tại hay không - chống user enumeration).
        if (user is null || !user.IsActive)
            return;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        // Tạo liên kết reset: {frontendUrl}/auth/reset-password?email={email}&token={token}
        var frontendUrl = "http://localhost:3000";
        var resetLink = $"{frontendUrl}/auth/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

        await emailSender.SendPasswordResetEmailAsync(user.Email!, resetLink, cancellationToken);
    }
}

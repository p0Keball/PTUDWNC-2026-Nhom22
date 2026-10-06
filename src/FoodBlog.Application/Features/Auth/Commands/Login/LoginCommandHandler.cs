using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FoodBlog.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<AuthResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto(
            string.Empty,
            string.Empty,
            DateTime.UtcNow.AddMinutes(15),
            new UserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                roles.FirstOrDefault() ?? "Author"));
    }
}
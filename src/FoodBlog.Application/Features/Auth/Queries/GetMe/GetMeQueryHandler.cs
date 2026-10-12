using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FoodBlog.Application.Features.Auth.Queries.GetMe;

public sealed class GetMeQueryHandler : IRequestHandler<GetMeQuery, UserProfileDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetMeQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserProfileDto> Handle(
        GetMeQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new UnauthorizedAccessException("Không xác định được người dùng.");

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.");

        var roles = await _userManager.GetRolesAsync(user);
        return new UserProfileDto(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.AvatarUrl,
            user.Bio,
            roles.FirstOrDefault() ?? "Author",
            user.IsActive,
            user.CreatedAt);
    }
}

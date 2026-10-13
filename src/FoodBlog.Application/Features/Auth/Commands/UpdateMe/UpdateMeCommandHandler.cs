using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FoodBlog.Application.Features.Auth.Commands.UpdateMe;

public sealed class UpdateMeCommandHandler : IRequestHandler<UpdateMeCommand, UserProfileDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateMeCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserProfileDto> Handle(
        UpdateMeCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new UnauthorizedAccessException("Không xác định được người dùng.");

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.");

        var payload = request.Request;
        if (payload.DisplayName is not null)
        {
            var displayName = payload.DisplayName.Trim();
            if (displayName.Length is < 2 or > 100)
                throw new ArgumentException("Tên hiển thị phải từ 2 đến 100 ký tự.");

            user.DisplayName = displayName;
        }

        if (payload.AvatarUrl is not null)
            user.AvatarUrl = string.IsNullOrWhiteSpace(payload.AvatarUrl)
                ? null
                : payload.AvatarUrl.Trim();

        if (payload.Bio is not null)
            user.Bio = string.IsNullOrWhiteSpace(payload.Bio)
                ? null
                : payload.Bio.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException(errors);
        }

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

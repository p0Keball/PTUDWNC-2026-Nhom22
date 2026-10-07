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
        {
            var avatarUrl = payload.AvatarUrl.Trim();
            if (avatarUrl.Length > 500)
                throw new ArgumentException("Ảnh đại diện không được vượt quá 500 ký tự.");

            if (!string.IsNullOrWhiteSpace(avatarUrl) &&
                (!Uri.TryCreate(avatarUrl, UriKind.Absolute, out var parsedAvatarUrl) ||
                 (parsedAvatarUrl.Scheme != Uri.UriSchemeHttp &&
                  parsedAvatarUrl.Scheme != Uri.UriSchemeHttps)))
                throw new ArgumentException("URL ảnh đại diện phải là địa chỉ HTTP hoặc HTTPS hợp lệ.");

            user.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl;
        }

        if (payload.Bio is not null)
        {
            var bio = payload.Bio.Trim();
            if (bio.Length > 1000)
                throw new ArgumentException("Giới thiệu không được vượt quá 1000 ký tự.");

            user.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio;
        }

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

using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.GoogleLogin;

public sealed record GoogleLoginCommand(
    string AccessToken,
    string? IpAddress) : IRequest<AuthResponseDto>;

using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.GoogleLogin;

public sealed record GoogleLoginCommand(
    string Code,
    string CodeVerifier,
    string? IpAddress) : IRequest<AuthResponseDto>;

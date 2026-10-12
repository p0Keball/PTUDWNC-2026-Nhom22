using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress
) : IRequest<AuthResponseDto>;  
using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponseDto>;

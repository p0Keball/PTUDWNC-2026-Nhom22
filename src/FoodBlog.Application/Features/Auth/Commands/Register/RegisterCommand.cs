using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FullName
) : IRequest<AuthResponseDto>;
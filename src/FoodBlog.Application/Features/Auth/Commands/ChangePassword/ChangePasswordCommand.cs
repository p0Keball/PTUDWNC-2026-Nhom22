using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string UserId,
    ChangePasswordRequest Request) : IRequest;

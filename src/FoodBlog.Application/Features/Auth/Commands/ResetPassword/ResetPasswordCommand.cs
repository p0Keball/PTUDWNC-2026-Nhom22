using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest;

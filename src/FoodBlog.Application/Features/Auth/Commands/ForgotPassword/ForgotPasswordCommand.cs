using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest;

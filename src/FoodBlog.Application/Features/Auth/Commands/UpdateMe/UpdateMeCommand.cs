using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.UpdateMe;

public sealed record UpdateMeCommand(
    string UserId,
    UpdateProfileRequest Request) : IRequest<UserProfileDto>;

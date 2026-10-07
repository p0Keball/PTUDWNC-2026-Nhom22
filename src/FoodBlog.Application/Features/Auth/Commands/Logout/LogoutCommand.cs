using MediatR;

namespace FoodBlog.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string UserId) : IRequest;

using FoodBlog.Application.DTOs.Auth;
using MediatR;

namespace FoodBlog.Application.Features.Auth.Queries.GetMe;

public sealed record GetMeQuery(string UserId) : IRequest<UserProfileDto>;

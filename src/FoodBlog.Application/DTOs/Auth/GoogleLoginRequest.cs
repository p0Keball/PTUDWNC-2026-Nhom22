namespace FoodBlog.Application.DTOs.Auth;

public sealed record GoogleLoginRequest(string Code, string CodeVerifier);

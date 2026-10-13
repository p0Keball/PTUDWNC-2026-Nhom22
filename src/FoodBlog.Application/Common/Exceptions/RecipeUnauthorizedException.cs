using System;

namespace FoodBlog.Application.Common.Exceptions;

[Obsolete("Dùng FoodBlog.Domain.Exceptions.RecipeUnauthorizedException.")]
public sealed class RecipeUnauthorizedException
    : FoodBlog.Domain.Exceptions.RecipeUnauthorizedException
{
}

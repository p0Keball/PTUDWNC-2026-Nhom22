using System;

namespace FoodBlog.Application.Common.Exceptions;

[Obsolete("Dùng FoodBlog.Domain.Exceptions.RecipeForbiddenException.")]
public sealed class RecipeForbiddenException
    : FoodBlog.Domain.Exceptions.RecipeForbiddenException
{
}

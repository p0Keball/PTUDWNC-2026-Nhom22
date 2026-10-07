using System;

namespace FoodBlog.Application.Common.Exceptions;

[Obsolete("Dùng FoodBlog.Domain.Exceptions.RecipeConcurrencyException.")]
public sealed class RecipeConcurrencyException
    : FoodBlog.Domain.Exceptions.RecipeConcurrencyException
{
}

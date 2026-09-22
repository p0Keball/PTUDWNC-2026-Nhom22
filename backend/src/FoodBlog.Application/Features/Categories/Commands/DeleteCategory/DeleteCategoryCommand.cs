using MediatR;

namespace FoodBlog.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>
/// Xóa danh mục — chỉ cần Id, không trả data (Unit = void trong MediatR).
/// FR-CAT-005: không xóa khi còn recipe (check ở handler).
/// </summary>
public record DeleteCategoryCommand(Guid Id) : IRequest<Unit>;

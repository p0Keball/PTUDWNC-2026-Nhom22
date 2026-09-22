using FoodBlog.Domain.Entities;

namespace FoodBlog.Domain.Interfaces;

/// <summary>
/// Abstraction Repository generic (Clean Architecture §1.2.2).
/// Application Layer chỉ phụ thuộc interface này, Infrastructure implement bằng EF Core.
/// </summary>
/// <typeparam name="T">Entity kế thừa BaseEntity</typeparam>
public interface IRepository<T> where T : Common.BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}

using System.ComponentModel.DataAnnotations;

namespace FoodBlog.Domain.Common;

/// <summary>
/// Base entity chung cho mọi aggregate (SRS §7.1).
/// Id: uuid PK, CreatedAt: timestamptz NOT NULL, UpdatedAt: nullable,
/// IsDeleted: soft-delete flag (Global Query Filter), RowVersion: concurrency token.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; protected set; }

    public bool IsDeleted { get; protected set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];
}

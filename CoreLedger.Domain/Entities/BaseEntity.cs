namespace CoreLedger.Domain.Entities;

/// <summary>
///     Base entity for all domain entities with common properties.
/// </summary>
public abstract class BaseEntity
{
    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    public void SetUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
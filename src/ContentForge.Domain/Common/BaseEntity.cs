namespace ContentForge.Domain.Common;

// Similar to a base class in TS: all entities extend this to get id + timestamps.
// "abstract" means you can't do `new BaseEntity()` directly — only subclasses can be instantiated.
public abstract class BaseEntity
{
    // Guid = UUID. Like crypto.randomUUID() in JS.
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // DateTime? = nullable. Like `Date | null` in TS.
    public DateTime? UpdatedAt { get; set; }
}

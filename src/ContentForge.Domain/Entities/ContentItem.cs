using ContentForge.Domain.Common;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Entities;

// This is like a Prisma model / Mongoose schema — defines a database table shape.
// `: BaseEntity` = extends BaseEntity (inherits Id, CreatedAt, UpdatedAt).
public class ContentItem : BaseEntity
{
    // { get; set; } = C# property syntax. Like a class field but with built-in getter/setter.
    // `= string.Empty` = default value, same as `= ""` in JS.
    public string BotName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    public string TextContent { get; set; } = string.Empty;
    public string? MediaPath { get; set; }
    public string? ThumbnailPath { get; set; }
    // Dictionary<string, string> = like Record<string, string> in TS. Stored as jsonb in Postgres.
    public Dictionary<string, string> Properties { get; set; } = new();

    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? LastError { get; set; }

    // Navigation properties — like Prisma relations. EF Core auto-joins these from the DB.
    // ICollection<T> is like T[] but with add/remove methods (think Set-like interface).
    public ICollection<PublishRecord> PublishRecords { get; set; } = new List<PublishRecord>();
    public ICollection<ContentMetric> Metrics { get; set; } = new List<ContentMetric>();

    // Graph feature navigation properties — extracted entities and cluster memberships.
    public ICollection<ContentEntity> Entities { get; set; } = new List<ContentEntity>();
    public ICollection<ContentClusterMember> ClusterMemberships { get; set; } = new List<ContentClusterMember>();
}

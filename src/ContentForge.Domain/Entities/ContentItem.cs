using ContentForge.Domain.Common;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Entities;

public class ContentItem : BaseEntity
{
    public string BotName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    public string TextContent { get; set; } = string.Empty;
    public string? MediaPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();

    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? LastError { get; set; }

    public ICollection<PublishRecord> PublishRecords { get; set; } = new List<PublishRecord>();
    public ICollection<ContentMetric> Metrics { get; set; } = new List<ContentMetric>();
}

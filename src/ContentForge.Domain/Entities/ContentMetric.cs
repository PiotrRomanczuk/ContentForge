using ContentForge.Domain.Common;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Entities;

public class ContentMetric : BaseEntity
{
    public Guid ContentItemId { get; set; }
    public ContentItem ContentItem { get; set; } = null!;

    public Platform Platform { get; set; }
    public string ExternalPostId { get; set; } = string.Empty;

    public int Impressions { get; set; }
    public int Reach { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public int Saves { get; set; }
    public double EngagementRate { get; set; }

    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> RawData { get; set; } = new();
}

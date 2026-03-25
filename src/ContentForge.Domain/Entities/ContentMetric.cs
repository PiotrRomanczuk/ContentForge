using ContentForge.Domain.Common;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Entities;

// Analytics snapshot — fetched from Meta Graph API and stored for dashboard display.
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
    // Raw API response data, stored as jsonb — like keeping the full JSON from the API "just in case".
    public Dictionary<string, string> RawData { get; set; } = new();
}

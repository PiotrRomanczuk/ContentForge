using ContentForge.Domain.Common;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Entities;

// Represents a connected social media account (like an OAuth connection stored in DB).
public class SocialAccount : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Platform Platform { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Dictionary<string, string> Metadata { get; set; } = new();

    // Navigation properties — EF auto-loads related records, like Prisma's `include`.
    public ICollection<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
    public ICollection<PublishRecord> PublishRecords { get; set; } = new List<PublishRecord>();
}

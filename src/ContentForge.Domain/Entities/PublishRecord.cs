using ContentForge.Domain.Common;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Entities;

// Logs each publish attempt — like an audit trail for "we tried to post this to Instagram at 3pm".
public class PublishRecord : BaseEntity
{
    // Foreign key — like a `contentItemId` column referencing the ContentItems table.
    public Guid ContentItemId { get; set; }
    // `= null!` tells the compiler "trust me, EF will populate this" — avoids null warnings.
    // This is the navigation property (the actual related object, not just the ID).
    public ContentItem ContentItem { get; set; } = null!;

    public Guid SocialAccountId { get; set; }
    public SocialAccount SocialAccount { get; set; } = null!;

    public Platform Platform { get; set; }
    public string? ExternalPostId { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}

using ContentForge.Domain.Common;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Entities;

public class PublishRecord : BaseEntity
{
    public Guid ContentItemId { get; set; }
    public ContentItem ContentItem { get; set; } = null!;

    public Guid SocialAccountId { get; set; }
    public SocialAccount SocialAccount { get; set; } = null!;

    public Platform Platform { get; set; }
    public string? ExternalPostId { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}

using ContentForge.Domain.Common;

namespace ContentForge.Domain.Entities;

// Junction table linking content items to clusters (many-to-many relationship).
// Like a Prisma implicit many-to-many relation, but explicit here for the membership score.
public class ContentClusterMember : BaseEntity
{
    public Guid ContentClusterId { get; set; }
    public Guid ContentItemId { get; set; }

    // How strongly this content item belongs to this cluster (0.0–1.0).
    // Derived from shared entity salience weights during Louvain community detection.
    public double MembershipScore { get; set; }

    // Navigation properties — EF Core auto-joins these.
    public ContentCluster Cluster { get; set; } = null!;
    public ContentItem ContentItem { get; set; } = null!;
}

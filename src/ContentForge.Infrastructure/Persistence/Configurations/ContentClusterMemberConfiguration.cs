using ContentForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentForge.Infrastructure.Persistence.Configurations;

// EF configuration for the cluster membership junction table.
// Maps the many-to-many relationship between content items and clusters.
public class ContentClusterMemberConfiguration : IEntityTypeConfiguration<ContentClusterMember>
{
    public void Configure(EntityTypeBuilder<ContentClusterMember> builder)
    {
        builder.HasKey(m => m.Id);

        builder.HasOne(m => m.Cluster)
            .WithMany()
            .HasForeignKey(m => m.ContentClusterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.ContentItem)
            .WithMany(ci => ci.ClusterMemberships)
            .HasForeignKey(m => m.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite unique index — a content item can only be in a cluster once.
        builder.HasIndex(m => new { m.ContentClusterId, m.ContentItemId }).IsUnique();
        builder.HasIndex(m => m.ContentItemId);
    }
}

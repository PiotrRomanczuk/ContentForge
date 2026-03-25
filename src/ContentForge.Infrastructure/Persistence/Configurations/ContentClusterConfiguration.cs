using ContentForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentForge.Infrastructure.Persistence.Configurations;

// EF configuration for ContentCluster — Louvain community detection results.
public class ContentClusterConfiguration : IEntityTypeConfiguration<ContentCluster>
{
    public void Configure(EntityTypeBuilder<ContentCluster> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Label).HasMaxLength(500).IsRequired();
        builder.Property(c => c.CommunityId).IsRequired();

        // Store List<string> as jsonb — like a JSON array of entity names.
        builder.Property(c => c.TopEntities)
            .HasColumnType("jsonb");

        builder.HasIndex(c => c.CommunityId);
    }
}

using ContentForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentForge.Infrastructure.Persistence.Configurations;

// EF configuration for ContentEntity — like a Prisma/Mongoose schema for entity graph nodes.
// Maps to a "ContentEntities" table with indexes for fast graph lookups.
public class ContentEntityConfiguration : IEntityTypeConfiguration<ContentEntity>
{
    public void Configure(EntityTypeBuilder<ContentEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.NormalizedName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.SalienceScore).IsRequired();

        // FK relationship: each entity belongs to one content item.
        // .HasMany(ci => ci.Entities).WithOne(e => e.ContentItem) = 1:N like Prisma relations.
        builder.HasOne(e => e.ContentItem)
            .WithMany(ci => ci.Entities)
            .HasForeignKey(e => e.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on NormalizedName — critical for finding shared entities across content items.
        // This is the key lookup for building graph edges in Louvain.
        builder.HasIndex(e => e.NormalizedName);
        builder.HasIndex(e => e.ContentItemId);

        // Composite index for efficient "find all items sharing entity X" queries.
        builder.HasIndex(e => new { e.NormalizedName, e.ContentItemId });
    }
}

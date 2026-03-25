using ContentForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentForge.Infrastructure.Persistence.Configurations;

// EF configuration for ContentEmbedding — stores vector embeddings for KNN search.
// The Vector column stores a float[] as jsonb. When pgvector is installed,
// this can be changed to `vector(N)` column type for native similarity operators.
public class ContentEmbeddingConfiguration : IEntityTypeConfiguration<ContentEmbedding>
{
    public void Configure(EntityTypeBuilder<ContentEmbedding> builder)
    {
        builder.HasKey(e => e.Id);

        // Store float[] as jsonb in PostgreSQL — like JSON.stringify(floatArray).
        // With pgvector extension, this would be: .HasColumnType("vector(384)")
        // for native cosine distance operators (<=> in SQL).
        builder.Property(e => e.Vector)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.ModelName).HasMaxLength(100).IsRequired();

        // One embedding per content item (1:1 relationship).
        builder.HasOne(e => e.ContentItem)
            .WithMany()
            .HasForeignKey(e => e.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index — only one embedding per content item.
        builder.HasIndex(e => e.ContentItemId).IsUnique();
    }
}

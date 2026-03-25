using ContentForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentForge.Infrastructure.Persistence.Configurations;

// EF entity configuration — like a Prisma schema or Mongoose schema definition.
// Defines column types, constraints, and indexes for the ContentItems table.
// Auto-discovered by ApplyConfigurationsFromAssembly() in DbContext.
public class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.HasKey(c => c.Id);

        // Fluent API: chaining .HasMaxLength().IsRequired() = column constraints.
        // Like: name VARCHAR(100) NOT NULL in raw SQL.
        builder.Property(c => c.BotName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Category).HasMaxLength(100).IsRequired();
        builder.Property(c => c.TextContent).IsRequired();
        builder.Property(c => c.MediaPath).HasMaxLength(500);
        builder.Property(c => c.ThumbnailPath).HasMaxLength(500);
        builder.Property(c => c.LastError).HasMaxLength(2000);

        // Store Dictionary<string,string> as PostgreSQL jsonb column.
        builder.Property(c => c.Properties)
            .HasColumnType("jsonb");

        // HasConversion<string>() = store enum as its name ("Draft", "Generated")
        // instead of as an integer (0, 1). Makes the DB human-readable.
        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.ContentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Database indexes — like CREATE INDEX. Speeds up WHERE queries on these columns.
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.BotName);
        builder.HasIndex(c => c.ScheduledAt);
        builder.HasIndex(c => c.CreatedAt);
    }
}

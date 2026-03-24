using ContentForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentForge.Infrastructure.Persistence.Configurations;

public class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.BotName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Category).HasMaxLength(100).IsRequired();
        builder.Property(c => c.TextContent).IsRequired();
        builder.Property(c => c.MediaPath).HasMaxLength(500);
        builder.Property(c => c.ThumbnailPath).HasMaxLength(500);
        builder.Property(c => c.LastError).HasMaxLength(2000);

        builder.Property(c => c.Properties)
            .HasColumnType("jsonb");

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.ContentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.BotName);
        builder.HasIndex(c => c.ScheduledAt);
        builder.HasIndex(c => c.CreatedAt);
    }
}

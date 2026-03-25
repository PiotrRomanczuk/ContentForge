using ContentForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentForge.Infrastructure.Persistence.Configurations;

public class SocialAccountConfiguration : IEntityTypeConfiguration<SocialAccount>
{
    public void Configure(EntityTypeBuilder<SocialAccount> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.ExternalId).HasMaxLength(200).IsRequired();
        builder.Property(a => a.AccessToken).HasMaxLength(1000).IsRequired();

        builder.Property(a => a.Platform)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Metadata)
            .HasColumnType("jsonb");

        // Composite unique index — ensures no two rows have the same (Platform, ExternalId) pair.
        // Like: CREATE UNIQUE INDEX ON social_accounts (platform, external_id)
        // `new { a.Platform, a.ExternalId }` = anonymous object (like an object literal in JS).
        builder.HasIndex(a => new { a.Platform, a.ExternalId }).IsUnique();
    }
}

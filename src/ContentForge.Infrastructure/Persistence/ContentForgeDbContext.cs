using ContentForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContentForge.Infrastructure.Persistence;

public class ContentForgeDbContext : DbContext
{
    public ContentForgeDbContext(DbContextOptions<ContentForgeDbContext> options)
        : base(options) { }

    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<SocialAccount> SocialAccounts => Set<SocialAccount>();
    public DbSet<BotRegistration> BotRegistrations => Set<BotRegistration>();
    public DbSet<ScheduleConfig> ScheduleConfigs => Set<ScheduleConfig>();
    public DbSet<PublishRecord> PublishRecords => Set<PublishRecord>();
    public DbSet<ContentMetric> ContentMetrics => Set<ContentMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentForgeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

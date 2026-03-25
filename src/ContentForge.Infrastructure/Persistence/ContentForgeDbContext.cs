using ContentForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContentForge.Infrastructure.Persistence;

// DbContext = the main EF Core class. Think of it as your Prisma client or Sequelize instance.
// It represents a session with the database — tracks changes, generates SQL, manages transactions.
// `: base(options)` = calls the parent constructor (like super(options) in JS).
public class ContentForgeDbContext : DbContext
{
    public ContentForgeDbContext(DbContextOptions<ContentForgeDbContext> options)
        : base(options) { }

    // Each DbSet<T> = a table in PostgreSQL. Like prisma.contentItem or db.collection('contentItems').
    // `=> Set<T>()` is a shorthand property (expression-bodied member) — like a getter arrow function.
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<SocialAccount> SocialAccounts => Set<SocialAccount>();
    public DbSet<BotRegistration> BotRegistrations => Set<BotRegistration>();
    public DbSet<ScheduleConfig> ScheduleConfigs => Set<ScheduleConfig>();
    public DbSet<PublishRecord> PublishRecords => Set<PublishRecord>();
    public DbSet<ContentMetric> ContentMetrics => Set<ContentMetric>();

    // Called once at startup to configure the DB schema. Like defining your Prisma schema in code.
    // `override` = overriding a virtual method from the base class (like overriding a method in JS class).
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-discovers all IEntityTypeConfiguration classes in this assembly (ContentItemConfiguration, etc.)
        // Like auto-loading all schema files from a folder.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentForgeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

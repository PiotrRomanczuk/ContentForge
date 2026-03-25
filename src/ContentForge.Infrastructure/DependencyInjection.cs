using ContentForge.Application.Services;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Domain.Interfaces.Services;
using ContentForge.Infrastructure.Persistence;
using ContentForge.Infrastructure.Persistence.Repositories;
using ContentForge.Infrastructure.Services;
using ContentForge.Infrastructure.Services.Media;
using ContentForge.Infrastructure.Services.Publishing;
using ContentForge.Infrastructure.Services.Scheduling;
using ContentForge.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Polly;
using Polly.Extensions.Http;

namespace ContentForge.Infrastructure;

// Extension method class — adds an `.AddInfrastructure()` method to IServiceCollection.
// Like a plugin: `app.use(infraPlugin)` in Express.
// `static class` = can't be instantiated, only has static methods (like a utility module in JS).
public static class DependencyInjection
{
    // `this IServiceCollection` = extension method. Lets you call services.AddInfrastructure()
    // as if it were a built-in method. Like monkey-patching a prototype in JS, but type-safe.
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Register EF Core with PostgreSQL. Like setting up Prisma/Sequelize connection.
        // AddDbContext = "create a new DB context per HTTP request" (Scoped lifetime).
        // NpgsqlDataSourceBuilder configures the Npgsql driver before EF Core uses it.
        // EnableDynamicJson() = opt-in for serializing CLR types (like Dictionary<string,string>)
        // to/from PostgreSQL jsonb columns. Required since Npgsql 8+ (was implicit before).
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ContentForgeDbContext>(options =>
            options.UseNpgsql(
                dataSource,
                b => b.MigrationsAssembly(typeof(ContentForgeDbContext).Assembly.FullName)));

        // DI registration: "when someone asks for IRepository<T>, give them Repository<T>".
        // AddScoped = new instance per HTTP request (like per-request middleware state in Express).
        // typeof(IRepository<>) = open generic — works for any T (ContentItem, SocialAccount, etc.)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IContentItemRepository, ContentItemRepository>();

        // AddSingleton = one instance for the entire app lifetime (like a module-level const in Node).
        services.AddSingleton<IBotRegistry, BotRegistry>();

        // Media rendering — ImageSharp-based renderer for social media images.
        // Configure<T>() binds a config section to a POCO. Like dotenv + typed config in Node.
        services.Configure<MediaRenderingOptions>(configuration.GetSection("MediaRendering"));
        services.AddScoped<IMediaRenderer, ImageSharpMediaRenderer>();

        // Publishing — repositories and platform adapters
        services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
        services.AddScoped<IPublishRecordRepository, PublishRecordRepository>();

        // Facebook adapter with Polly retry — like axios-retry with exponential backoff.
        // AddHttpClient creates a named, pooled HttpClient (avoids socket exhaustion).
        // AddTransientHttpErrorPolicy adds automatic retry on 5xx and network errors.
        services.Configure<FacebookOptions>(configuration.GetSection("Facebook"));
        services.AddHttpClient("Facebook")
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddScoped<IPlatformAdapter, FacebookPlatformAdapter>();
        services.AddScoped<IPlatformAdapterFactory, PlatformAdapterFactory>();

        // Security — token encryption for social account access tokens at rest
        services.AddDataProtection();
        services.AddScoped<ITokenEncryptionService, DataProtectionTokenEncryptionService>();

        // Scheduling — repositories and Hangfire job management
        services.AddScoped<IScheduleConfigRepository, ScheduleConfigRepository>();
        services.AddScoped<IScheduleJobManager, HangfireScheduleJobManager>();
        services.AddScoped<AutoPublishJob>();
        services.AddScoped<MetricsCollectionJob>();

        return services;
    }
}

using ContentForge.Application.Services;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Infrastructure.Persistence;
using ContentForge.Infrastructure.Persistence.Repositories;
using ContentForge.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddDbContext<ContentForgeDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ContentForgeDbContext).Assembly.FullName)));

        // DI registration: "when someone asks for IRepository<T>, give them Repository<T>".
        // AddScoped = new instance per HTTP request (like per-request middleware state in Express).
        // typeof(IRepository<>) = open generic — works for any T (ContentItem, SocialAccount, etc.)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IContentItemRepository, ContentItemRepository>();

        // AddSingleton = one instance for the entire app lifetime (like a module-level const in Node).
        services.AddSingleton<IBotRegistry, BotRegistry>();

        return services;
    }
}

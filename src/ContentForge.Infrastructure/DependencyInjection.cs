using ContentForge.Application.Services;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Infrastructure.Persistence;
using ContentForge.Infrastructure.Persistence.Repositories;
using ContentForge.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContentForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ContentForgeDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ContentForgeDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IContentItemRepository, ContentItemRepository>();

        // Services
        services.AddSingleton<IBotRegistry, BotRegistry>();

        return services;
    }
}

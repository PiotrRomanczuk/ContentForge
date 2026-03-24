using ContentForge.Application.Services;
using ContentForge.Bots.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace ContentForge.Bots;

public static class DependencyInjection
{
    public static IServiceCollection AddBots(this IServiceCollection services)
    {
        services.AddSingleton<EnglishFactsBot>();
        services.AddSingleton<HoroscopeBot>();
        return services;
    }

    public static void InitializeBots(IServiceProvider serviceProvider)
    {
        var registry = serviceProvider.GetRequiredService<IBotRegistry>();
        registry.Register(serviceProvider.GetRequiredService<EnglishFactsBot>());
        registry.Register(serviceProvider.GetRequiredService<HoroscopeBot>());
    }
}

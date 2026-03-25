using ContentForge.Application.Services;
using ContentForge.Bots.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace ContentForge.Bots;

public static class DependencyInjection
{
    // Step 1: Register bot classes in the DI container (called during app startup).
    public static IServiceCollection AddBots(this IServiceCollection services)
    {
        services.AddSingleton<EnglishFactsBot>();
        services.AddSingleton<HoroscopeBot>();
        return services;
    }

    // Step 2: After the app is built, pull bots from DI and register them in the BotRegistry.
    // This two-phase init is needed because the registry itself is also a DI singleton —
    // you can't register bots *into* it until the DI container is fully built.
    // GetRequiredService<T>() = like container.resolve(Token) in NestJS. Throws if not found.
    public static void InitializeBots(IServiceProvider serviceProvider)
    {
        var registry = serviceProvider.GetRequiredService<IBotRegistry>();
        registry.Register(serviceProvider.GetRequiredService<EnglishFactsBot>());
        registry.Register(serviceProvider.GetRequiredService<HoroscopeBot>());
    }
}

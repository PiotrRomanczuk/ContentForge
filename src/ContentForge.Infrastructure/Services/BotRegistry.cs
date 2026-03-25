using System.Collections.Concurrent;
using ContentForge.Application.Services;
using ContentForge.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Services;

// Singleton service — one instance for the whole app (registered via AddSingleton in DI).
public class BotRegistry : IBotRegistry
{
    // ConcurrentDictionary = thread-safe Map. Like `new Map()` but safe for parallel access.
    // Needed because this is a singleton potentially accessed by multiple request threads at once.
    private readonly ConcurrentDictionary<string, IBotDefinition> _bots = new();
    // ILogger<T> = .NET's built-in logger. Like `const logger = winston.createLogger()` scoped to this class.
    // Structured logging: {BotName} creates a named parameter, not just string interpolation.
    private readonly ILogger<BotRegistry> _logger;

    public BotRegistry(ILogger<BotRegistry> logger)
    {
        _logger = logger;
    }

    public void Register(IBotDefinition bot)
    {
        // TryAdd = like map.set() but returns false if key already exists (no overwrite).
        if (_bots.TryAdd(bot.Name, bot))
            _logger.LogInformation("Registered bot '{BotName}' (category: {Category})", bot.Name, bot.Category);
        else
            _logger.LogWarning("Bot '{BotName}' is already registered", bot.Name);
    }

    // `out var bot` = like destructuring a return value. TryGetValue returns bool + sets `bot`.
    // Ternary returns the bot if found, null if not. Like: map.get(name) ?? null
    public IBotDefinition? GetBot(string name)
        => _bots.TryGetValue(name, out var bot) ? bot : null;

    public IReadOnlyList<IBotDefinition> GetAllBots()
        => _bots.Values.ToList();

    public IReadOnlyList<string> GetRegisteredBotNames()
        => _bots.Keys.ToList();
}

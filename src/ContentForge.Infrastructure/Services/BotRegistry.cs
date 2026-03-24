using System.Collections.Concurrent;
using ContentForge.Application.Services;
using ContentForge.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Services;

public class BotRegistry : IBotRegistry
{
    private readonly ConcurrentDictionary<string, IBotDefinition> _bots = new();
    private readonly ILogger<BotRegistry> _logger;

    public BotRegistry(ILogger<BotRegistry> logger)
    {
        _logger = logger;
    }

    public void Register(IBotDefinition bot)
    {
        if (_bots.TryAdd(bot.Name, bot))
            _logger.LogInformation("Registered bot '{BotName}' (category: {Category})", bot.Name, bot.Category);
        else
            _logger.LogWarning("Bot '{BotName}' is already registered", bot.Name);
    }

    public IBotDefinition? GetBot(string name)
        => _bots.TryGetValue(name, out var bot) ? bot : null;

    public IReadOnlyList<IBotDefinition> GetAllBots()
        => _bots.Values.ToList();

    public IReadOnlyList<string> GetRegisteredBotNames()
        => _bots.Keys.ToList();
}

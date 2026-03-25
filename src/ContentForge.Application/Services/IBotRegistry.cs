using ContentForge.Domain.Interfaces.Services;

namespace ContentForge.Application.Services;

// In-memory registry of all available bots. Like a Map<string, BotDefinition> singleton.
// Lives in Application layer (interface), implemented in Infrastructure (BotRegistry.cs).
// This separation lets the domain/application code use bots without knowing how they're stored.
public interface IBotRegistry
{
    void Register(IBotDefinition bot);
    IBotDefinition? GetBot(string name);
    IReadOnlyList<IBotDefinition> GetAllBots();
    IReadOnlyList<string> GetRegisteredBotNames();
}

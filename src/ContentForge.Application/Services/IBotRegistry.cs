using ContentForge.Domain.Interfaces.Services;

namespace ContentForge.Application.Services;

public interface IBotRegistry
{
    void Register(IBotDefinition bot);
    IBotDefinition? GetBot(string name);
    IReadOnlyList<IBotDefinition> GetAllBots();
    IReadOnlyList<string> GetRegisteredBotNames();
}

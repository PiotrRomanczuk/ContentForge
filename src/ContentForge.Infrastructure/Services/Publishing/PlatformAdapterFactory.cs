using ContentForge.Application.Services;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Services.Publishing;

// Resolves the correct IPlatformAdapter by Platform enum.
// IEnumerable<IPlatformAdapter> is auto-injected by DI — contains all registered adapters.
// Like a Map<Platform, Adapter> built from DI container resolution.
public class PlatformAdapterFactory : IPlatformAdapterFactory
{
    private readonly Dictionary<Platform, IPlatformAdapter> _adapters;
    private readonly ILogger<PlatformAdapterFactory> _logger;

    public PlatformAdapterFactory(
        IEnumerable<IPlatformAdapter> adapters,
        ILogger<PlatformAdapterFactory> logger)
    {
        _adapters = adapters.ToDictionary(a => a.Platform);
        _logger = logger;
    }

    public IPlatformAdapter GetAdapter(Platform platform)
    {
        if (_adapters.TryGetValue(platform, out var adapter))
        {
            _logger.LogDebug("Resolved adapter for platform {Platform}", platform);
            return adapter;
        }

        _logger.LogWarning("No adapter registered for platform {Platform}. Supported: {Supported}",
            platform, string.Join(", ", _adapters.Keys));
        throw new NotSupportedException(
            $"No adapter registered for platform '{platform}'. " +
            $"Supported: {string.Join(", ", _adapters.Keys)}");
    }

    public IReadOnlyList<Platform> GetSupportedPlatforms()
        => _adapters.Keys.ToList();
}

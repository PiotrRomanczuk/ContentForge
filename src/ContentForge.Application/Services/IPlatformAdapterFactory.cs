using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Services;

namespace ContentForge.Application.Services;

// Factory pattern — resolves the correct IPlatformAdapter for a given Platform.
// Like a strategy/adapter resolver: adapters['Facebook'] → FacebookPlatformAdapter.
// Lives in Application (not Domain) because it's an application-level concern.
public interface IPlatformAdapterFactory
{
    IPlatformAdapter GetAdapter(Platform platform);
    IReadOnlyList<Platform> GetSupportedPlatforms();
}

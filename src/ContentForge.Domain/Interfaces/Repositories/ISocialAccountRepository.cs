using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Interfaces.Repositories;

public interface ISocialAccountRepository : IRepository<SocialAccount>
{
    Task<IReadOnlyList<SocialAccount>> GetByPlatformAsync(
        Platform platform, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialAccount>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}

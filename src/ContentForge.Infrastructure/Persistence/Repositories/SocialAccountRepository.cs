using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Persistence.Repositories;

// Extends the generic Repository<T> with SocialAccount-specific queries.
// Like adding custom methods to a Prisma model extension.
public class SocialAccountRepository : Repository<SocialAccount>, ISocialAccountRepository
{
    public SocialAccountRepository(ContentForgeDbContext context, ILoggerFactory? loggerFactory = null)
        : base(context, loggerFactory) { }

    public async Task<IReadOnlyList<SocialAccount>> GetByPlatformAsync(
        Platform platform, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("GetByPlatform({Platform})", platform);
        return await DbSet.Where(a => a.Platform == platform)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SocialAccount>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("GetActive()");
        return await DbSet.Where(a => a.IsActive)
            .ToListAsync(cancellationToken);
    }
}

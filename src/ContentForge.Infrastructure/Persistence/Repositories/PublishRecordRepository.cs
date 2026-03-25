using ContentForge.Domain.Entities;
using ContentForge.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Persistence.Repositories;

public class PublishRecordRepository : Repository<PublishRecord>, IPublishRecordRepository
{
    public PublishRecordRepository(ContentForgeDbContext context, ILoggerFactory? loggerFactory = null)
        : base(context, loggerFactory) { }

    public async Task<IReadOnlyList<PublishRecord>> GetByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("GetByContentItemId({ContentItemId})", contentItemId);
        return await DbSet.Where(r => r.ContentItemId == contentItemId)
            .OrderByDescending(r => r.AttemptedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PublishRecord>> GetRecentlyPublishedAsync(
        DateTime since, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("GetRecentlyPublished(since: {Since})", since);
        return await DbSet.Where(r => r.IsSuccess && r.AttemptedAt >= since)
            .Include(r => r.ContentItem)
            .Include(r => r.SocialAccount)
            .ToListAsync(cancellationToken);
    }
}

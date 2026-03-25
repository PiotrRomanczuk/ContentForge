using ContentForge.Domain.Entities;
using ContentForge.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Persistence.Repositories;

// Repository for extracted content entities — queries the entity graph nodes.
public class ContentEntityRepository : Repository<ContentEntity>, IContentEntityRepository
{
    public ContentEntityRepository(ContentForgeDbContext context, ILoggerFactory? loggerFactory = null)
        : base(context, loggerFactory) { }

    public async Task<IReadOnlyList<ContentEntity>> GetByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.ContentItemId == contentItemId)
            .OrderByDescending(e => e.SalienceScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentEntity>> GetByNormalizedNameAsync(
        string normalizedName, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.NormalizedName == normalizedName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetDistinctEntityNamesAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Select(e => e.NormalizedName)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task AddEntitiesForContentAsync(
        Guid contentItemId, IEnumerable<ContentEntity> entities,
        CancellationToken cancellationToken = default)
    {
        // Remove old entities first (idempotent re-extraction).
        var existing = await DbSet
            .Where(e => e.ContentItemId == contentItemId)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
            DbSet.RemoveRange(existing);

        await DbSet.AddRangeAsync(entities, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }
}

using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ContentForge.Infrastructure.Persistence.Repositories;

// Extends the generic Repository with content-specific queries.
// `: base(context)` = super(context) — passes the DB context to the parent constructor.
public class ContentItemRepository : Repository<ContentItem>, IContentItemRepository
{
    public ContentItemRepository(ContentForgeDbContext context) : base(context) { }

    // LINQ query chain — reads like: DbSet.filter().sort().toArray()
    // .Where() = .filter(), .OrderByDescending() = .sort((a,b) => b.date - a.date)
    // EF translates this chain into a single SQL query (SELECT ... WHERE ... ORDER BY ...).
    public async Task<IReadOnlyList<ContentItem>> GetByStatusAsync(
        ContentStatus status, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ContentItem>> GetByBotNameAsync(
        string botName, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(c => c.BotName == botName)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    // .Skip() + .Take() = pagination, like .offset().limit() in Prisma/Knex.
    public async Task<IReadOnlyList<ContentItem>> GetPendingApprovalAsync(
        int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(c => c.Status == ContentStatus.Generated || c.Status == ContentStatus.Rendered)
            .OrderBy(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ContentItem>> GetScheduledForPublishingAsync(
        DateTime until, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(c => c.Status == ContentStatus.Queued
                        && c.ScheduledAt != null
                        && c.ScheduledAt <= until)
            .OrderBy(c => c.ScheduledAt)
            .ToListAsync(cancellationToken);

    public async Task<int> GetCountByStatusAsync(
        ContentStatus status, CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(c => c.Status == status, cancellationToken);

    // ExecuteUpdateAsync = bulk UPDATE in one SQL statement (no loading entities into memory).
    // Like: UPDATE content_items SET status = $1, updated_at = $2 WHERE id = ANY($3)
    // Much faster than loading + modifying + saving each entity individually.
    public async Task BulkUpdateStatusAsync(
        IEnumerable<Guid> ids, ContentStatus status, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        await DbSet
            .Where(c => idList.Contains(c.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Status, status)
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }
}

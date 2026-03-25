using ContentForge.Domain.Entities;
using ContentForge.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Persistence.Repositories;

// Repository for Louvain clustering results — stores discovered content communities.
public class ContentClusterRepository : Repository<ContentCluster>, IContentClusterRepository
{
    public ContentClusterRepository(ContentForgeDbContext context, ILoggerFactory? loggerFactory = null)
        : base(context, loggerFactory) { }

    public async Task<IReadOnlyList<ContentCluster>> GetAllWithMembersAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderByDescending(c => c.MemberCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<ContentCluster?> GetByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        // Find via the junction table — which cluster does this content item belong to?
        var membership = await Context.Set<ContentClusterMember>()
            .Include(m => m.Cluster)
            .FirstOrDefaultAsync(m => m.ContentItemId == contentItemId, cancellationToken);

        return membership?.Cluster;
    }

    public async Task<IReadOnlyList<ContentClusterMember>> GetClusterMembersAsync(
        Guid clusterId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ContentClusterMember>()
            .Where(m => m.ContentClusterId == clusterId)
            .Include(m => m.ContentItem)
            .OrderByDescending(m => m.MembershipScore)
            .ToListAsync(cancellationToken);
    }

    // Replace all clusters — Louvain re-runs regenerate the entire clustering.
    // Like dropping and recreating a materialized view.
    public async Task ReplaceAllAsync(
        IEnumerable<ContentCluster> clusters,
        IEnumerable<ContentClusterMember> memberships,
        CancellationToken cancellationToken = default)
    {
        // Clear existing memberships and clusters.
        var existingMemberships = await Context.Set<ContentClusterMember>()
            .ToListAsync(cancellationToken);
        Context.Set<ContentClusterMember>().RemoveRange(existingMemberships);

        var existingClusters = await DbSet.ToListAsync(cancellationToken);
        DbSet.RemoveRange(existingClusters);

        // Insert new clusters and memberships.
        await DbSet.AddRangeAsync(clusters, cancellationToken);
        await Context.Set<ContentClusterMember>().AddRangeAsync(memberships, cancellationToken);

        await Context.SaveChangesAsync(cancellationToken);
    }
}

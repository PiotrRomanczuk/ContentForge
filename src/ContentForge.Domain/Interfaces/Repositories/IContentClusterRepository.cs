using ContentForge.Domain.Entities;

namespace ContentForge.Domain.Interfaces.Repositories;

// Repository for content clusters — stores Louvain community detection results.
public interface IContentClusterRepository : IRepository<ContentCluster>
{
    // Get all clusters with their member counts.
    Task<IReadOnlyList<ContentCluster>> GetAllWithMembersAsync(
        CancellationToken cancellationToken = default);

    // Get the cluster a content item belongs to.
    Task<ContentCluster?> GetByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);

    // Get all content items in a specific cluster.
    Task<IReadOnlyList<ContentClusterMember>> GetClusterMembersAsync(
        Guid clusterId, CancellationToken cancellationToken = default);

    // Replace all clusters and memberships (after re-running Louvain).
    Task ReplaceAllAsync(
        IEnumerable<ContentCluster> clusters,
        IEnumerable<ContentClusterMember> memberships,
        CancellationToken cancellationToken = default);
}

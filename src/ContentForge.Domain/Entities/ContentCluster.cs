using ContentForge.Domain.Common;

namespace ContentForge.Domain.Entities;

// A cluster/community of related content items, discovered by Louvain community detection.
// The Louvain algorithm finds groups of densely connected nodes in a graph.
// Here, content items are nodes and their shared entities (weighted by salience) form edges.
// Think of it like auto-generated tags/topics, but discovered from the content graph structure.
public class ContentCluster : BaseEntity
{
    // Human-readable label for this cluster (e.g., "European History", "Space Science").
    // Can be auto-generated from the top entities in the cluster.
    public string Label { get; set; } = string.Empty;

    // The Louvain community ID — integer assigned by the algorithm.
    public int CommunityId { get; set; }

    // Modularity score for this cluster — measures how well-separated it is from others.
    // Higher = tighter, more cohesive cluster. Range: -0.5 to 1.0.
    public double ModularityScore { get; set; }

    // Top entities that define this cluster (stored as JSON array of entity names).
    // Like the "keywords" or "tags" that best describe this topic group.
    public List<string> TopEntities { get; set; } = new();

    // Number of content items in this cluster.
    public int MemberCount { get; set; }
}

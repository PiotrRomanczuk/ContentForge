using ContentForge.Domain.Entities;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Services.Graph;

// Implementation of the content knowledge graph service.
// Combines three techniques:
// 1. Entity extraction — TF-IDF-based keyword/entity extraction with salience scoring
// 2. Embedding generation — TF-IDF vector embeddings for KNN similarity search
// 3. Louvain clustering — community detection on the entity co-occurrence graph
//
// This is a self-contained implementation (no external NLP APIs required).
// For production, you could swap in OpenAI/Claude embeddings by implementing a different service.
public class ContentGraphService : IContentGraphService
{
    private readonly IContentItemRepository _contentRepository;
    private readonly IContentEntityRepository _entityRepository;
    private readonly IContentEmbeddingRepository _embeddingRepository;
    private readonly IContentClusterRepository _clusterRepository;
    private readonly ILogger<ContentGraphService> _logger;

    public ContentGraphService(
        IContentItemRepository contentRepository,
        IContentEntityRepository entityRepository,
        IContentEmbeddingRepository embeddingRepository,
        IContentClusterRepository clusterRepository,
        ILogger<ContentGraphService> logger)
    {
        _contentRepository = contentRepository;
        _entityRepository = entityRepository;
        _embeddingRepository = embeddingRepository;
        _clusterRepository = clusterRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ContentEntity>> ExtractEntitiesAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        var item = await _contentRepository.GetByIdAsync(contentItemId, cancellationToken)
            ?? throw new InvalidOperationException($"Content item {contentItemId} not found");

        var entities = EntityExtractor.Extract(item.TextContent);

        // Set the FK on each entity before persisting.
        foreach (var entity in entities)
            entity.ContentItemId = contentItemId;

        await _entityRepository.AddEntitiesForContentAsync(contentItemId, entities, cancellationToken);

        _logger.LogInformation("Extracted {Count} entities from content {Id}",
            entities.Count, contentItemId);

        return entities;
    }

    public async Task<ContentEmbedding> GenerateEmbeddingAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        var item = await _contentRepository.GetByIdAsync(contentItemId, cancellationToken)
            ?? throw new InvalidOperationException($"Content item {contentItemId} not found");

        // Build TF-IDF vocabulary from all content for consistent vector dimensions.
        var allContent = await _contentRepository.GetAllAsync(cancellationToken);
        var corpus = allContent.Select(c => c.TextContent).ToList();

        var vector = TfIdfEmbedder.GenerateEmbedding(item.TextContent, corpus);

        // Upsert: replace existing embedding if one exists.
        var existing = await _embeddingRepository.GetByContentItemIdAsync(contentItemId, cancellationToken);
        if (existing is not null)
            await _embeddingRepository.DeleteAsync(existing, cancellationToken);

        var embedding = new ContentEmbedding
        {
            ContentItemId = contentItemId,
            Vector = vector,
            ModelName = "tfidf",
            Dimensions = vector.Length
        };

        await _embeddingRepository.AddAsync(embedding, cancellationToken);

        _logger.LogInformation("Generated {Dim}-dimensional embedding for content {Id}",
            vector.Length, contentItemId);

        return embedding;
    }

    public async Task<IReadOnlyList<(Guid ContentItemId, double SimilarityScore)>> FindSimilarContentAsync(
        Guid contentItemId, int k = 10, CancellationToken cancellationToken = default)
    {
        var embedding = await _embeddingRepository.GetByContentItemIdAsync(contentItemId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"No embedding found for content {contentItemId}. Run entity extraction first.");

        // KNN search — find nearest neighbors by cosine distance.
        // k+1 because the query item itself will be in the results.
        var nearest = await _embeddingRepository.FindNearestAsync(
            embedding.Vector, k + 1, cancellationToken);

        return nearest
            .Where(n => n.Embedding.ContentItemId != contentItemId) // Exclude self
            .Take(k)
            .Select(n => (
                ContentItemId: n.Embedding.ContentItemId,
                SimilarityScore: 1.0 - n.Distance)) // Convert distance to similarity
            .ToList();
    }

    public async Task<IReadOnlyList<ContentCluster>> RunClusteringAsync(
        CancellationToken cancellationToken = default)
    {
        // Step 1: Build the weighted graph from entity co-occurrences.
        // Nodes = content item IDs.
        // Edge weight between items A and B = sum of (salienceA * salienceB) for each shared entity.
        var allEntities = await _entityRepository.GetAllAsync(cancellationToken);

        // Group entities by normalized name to find co-occurrences.
        var entityGroups = allEntities
            .GroupBy(e => e.NormalizedName)
            .Where(g => g.Count() > 1) // Only entities appearing in multiple items form edges
            .ToList();

        // Build adjacency list: contentId → { neighborId → weight }
        var graph = new Dictionary<Guid, Dictionary<Guid, double>>();

        foreach (var group in entityGroups)
        {
            var items = group.ToList();
            // Create edges between all pairs of items sharing this entity.
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    var idA = items[i].ContentItemId;
                    var idB = items[j].ContentItemId;
                    // Edge weight = product of salience scores (higher = more related).
                    var weight = items[i].SalienceScore * items[j].SalienceScore;

                    AddEdge(graph, idA, idB, weight);
                    AddEdge(graph, idB, idA, weight);
                }
            }
        }

        if (graph.Count == 0)
        {
            _logger.LogWarning("No entity co-occurrences found. Run entity extraction on content first.");
            return Array.Empty<ContentCluster>();
        }

        // Step 2: Run Louvain community detection.
        var communities = LouvainAlgorithm.Detect(graph);

        // Step 3: Build cluster entities with labels from top entities.
        var clusters = new List<ContentCluster>();
        var memberships = new List<ContentClusterMember>();

        foreach (var (communityId, memberIds) in communities)
        {
            // Find top entities for this community — most salient entities shared by its members.
            var communityEntities = allEntities
                .Where(e => memberIds.Contains(e.ContentItemId))
                .GroupBy(e => e.NormalizedName)
                .OrderByDescending(g => g.Sum(e => e.SalienceScore))
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            var cluster = new ContentCluster
            {
                CommunityId = communityId,
                Label = string.Join(", ", communityEntities.Take(3)),
                TopEntities = communityEntities,
                MemberCount = memberIds.Count,
                // Modularity for this community (simplified — fraction of internal edges).
                ModularityScore = CalculateCommunityModularity(graph, memberIds)
            };

            clusters.Add(cluster);

            foreach (var memberId in memberIds)
            {
                // Membership score = how connected this item is within the community.
                var internalWeight = graph.GetValueOrDefault(memberId)?
                    .Where(kv => memberIds.Contains(kv.Key))
                    .Sum(kv => kv.Value) ?? 0;

                var totalWeight = graph.GetValueOrDefault(memberId)?
                    .Sum(kv => kv.Value) ?? 1;

                memberships.Add(new ContentClusterMember
                {
                    ContentClusterId = cluster.Id,
                    ContentItemId = memberId,
                    MembershipScore = totalWeight > 0 ? internalWeight / totalWeight : 0
                });
            }
        }

        // Step 4: Persist clusters and memberships.
        await _clusterRepository.ReplaceAllAsync(clusters, memberships, cancellationToken);

        _logger.LogInformation("Louvain clustering: {ClusterCount} communities from {NodeCount} content items",
            clusters.Count, graph.Count);

        return clusters;
    }

    private static void AddEdge(Dictionary<Guid, Dictionary<Guid, double>> graph,
        Guid from, Guid to, double weight)
    {
        if (!graph.ContainsKey(from))
            graph[from] = new Dictionary<Guid, double>();

        // Accumulate weights for multiple shared entities.
        graph[from].TryGetValue(to, out var existing);
        graph[from][to] = existing + weight;
    }

    // Simplified modularity: fraction of edges internal to this community.
    private static double CalculateCommunityModularity(
        Dictionary<Guid, Dictionary<Guid, double>> graph, HashSet<Guid> members)
    {
        double internalWeight = 0, totalWeight = 0;

        foreach (var member in members)
        {
            if (!graph.TryGetValue(member, out var neighbors)) continue;

            foreach (var (neighbor, weight) in neighbors)
            {
                totalWeight += weight;
                if (members.Contains(neighbor))
                    internalWeight += weight;
            }
        }

        return totalWeight > 0 ? internalWeight / totalWeight : 0;
    }
}

using ContentForge.Domain.Entities;

namespace ContentForge.Domain.Interfaces.Services;

// Service interface for the content knowledge graph — entity extraction, embedding, and clustering.
// Combines three techniques from the screenshot:
// 1. Entity anchoring — extract named entities with salience scores from content text
// 2. vec0 KNN — vector similarity search using embeddings
// 3. Salience-weighted Louvain — community detection on the entity co-occurrence graph
//
// Think of it as a smart content analyzer that can:
// - Tag content automatically (entity extraction)
// - Find similar content (KNN search)
// - Group content into topic clusters (Louvain communities)
public interface IContentGraphService
{
    // Extract named entities from content text and store them with salience scores.
    // Like running NER (Named Entity Recognition) — identifies people, places, topics, etc.
    Task<IReadOnlyList<ContentEntity>> ExtractEntitiesAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);

    // Generate a vector embedding for content text and store it for KNN search.
    // The embedding captures the semantic meaning of the text as a float array.
    Task<ContentEmbedding> GenerateEmbeddingAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);

    // Find the K most similar content items using vector KNN search.
    // Returns content IDs ranked by similarity (closest first).
    Task<IReadOnlyList<(Guid ContentItemId, double SimilarityScore)>> FindSimilarContentAsync(
        Guid contentItemId, int k = 10, CancellationToken cancellationToken = default);

    // Run Louvain community detection on the entity co-occurrence graph.
    // Builds a graph where: nodes = content items, edges = shared entities weighted by salience.
    // Returns discovered clusters/communities.
    Task<IReadOnlyList<ContentCluster>> RunClusteringAsync(
        CancellationToken cancellationToken = default);
}

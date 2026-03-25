using ContentForge.Domain.Entities;

namespace ContentForge.Domain.Interfaces.Repositories;

// Repository for content embeddings — provides KNN (k-nearest neighbor) vector search.
// Think of KNN as: "given this content, find the K most similar items by meaning."
// Uses PostgreSQL pgvector extension under the hood for efficient similarity queries.
public interface IContentEmbeddingRepository : IRepository<ContentEmbedding>
{
    // Get the embedding for a specific content item.
    Task<ContentEmbedding?> GetByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);

    // KNN search — find the K nearest content embeddings to a given vector.
    // Like: "find the 10 posts most similar to this one."
    // Uses cosine distance (1 - cosine_similarity) for ranking.
    Task<IReadOnlyList<(ContentEmbedding Embedding, double Distance)>> FindNearestAsync(
        float[] queryVector, int k = 10, CancellationToken cancellationToken = default);

    // Check if an embedding already exists for a content item (avoid duplicates).
    Task<bool> ExistsForContentItemAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);
}

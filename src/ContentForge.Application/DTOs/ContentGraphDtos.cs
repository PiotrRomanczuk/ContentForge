namespace ContentForge.Application.DTOs;

// DTOs for the content graph feature — entity extraction, similarity search, and clustering.
// All records (immutable) — like Object.freeze({ ... }) in JS.

// A single extracted entity with its salience score.
public record ContentEntityDto(
    Guid Id,
    Guid ContentItemId,
    string Name,
    string EntityType,
    double SalienceScore,
    int MentionCount);

// Result of entity extraction for a content item.
public record EntityExtractionResultDto(
    Guid ContentItemId,
    int EntityCount,
    IReadOnlyList<ContentEntityDto> Entities);

// A similar content item found via KNN vector search.
public record SimilarContentDto(
    Guid ContentItemId,
    string BotName,
    string Category,
    string TextContentPreview,
    double SimilarityScore);

// Result of a similarity search query.
public record SimilaritySearchResultDto(
    Guid QueryContentItemId,
    int ResultCount,
    IReadOnlyList<SimilarContentDto> SimilarItems);

// A content cluster discovered by Louvain community detection.
public record ContentClusterDto(
    Guid Id,
    string Label,
    int CommunityId,
    double ModularityScore,
    IReadOnlyList<string> TopEntities,
    int MemberCount);

// A content item's membership in a cluster.
public record ClusterMemberDto(
    Guid ContentItemId,
    string BotName,
    string TextContentPreview,
    double MembershipScore);

// Result of running Louvain clustering.
public record ClusteringResultDto(
    int ClusterCount,
    int TotalContentItems,
    double OverallModularity,
    IReadOnlyList<ContentClusterDto> Clusters);

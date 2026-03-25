using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.ContentGraph;

// Query to find content similar to a given item using KNN vector search.
// In CQRS, queries are read-only operations (no side effects).
// K = how many similar items to return (like LIMIT in SQL).
public record FindSimilarContentQuery(
    Guid ContentItemId,
    int K = 10
) : IRequest<SimilaritySearchResultDto>;

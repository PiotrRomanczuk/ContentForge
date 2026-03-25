using ContentForge.Application.DTOs;
using ContentForge.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.ContentGraph;

// Handler for Louvain clustering — runs community detection on the content entity graph.
// The Louvain algorithm iteratively merges nodes into communities to maximize modularity.
// Modularity = a metric measuring how well the graph is partitioned into communities.
public class RunClusteringHandler : IRequestHandler<RunClusteringCommand, ClusteringResultDto>
{
    private readonly IContentGraphService _graphService;
    private readonly ILogger<RunClusteringHandler> _logger;

    public RunClusteringHandler(
        IContentGraphService graphService,
        ILogger<RunClusteringHandler> logger)
    {
        _graphService = graphService;
        _logger = logger;
    }

    public async Task<ClusteringResultDto> Handle(
        RunClusteringCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running Louvain community detection on content graph");

        var clusters = await _graphService.RunClusteringAsync(cancellationToken);

        var totalMembers = clusters.Sum(c => c.MemberCount);
        var overallModularity = clusters.Count > 0
            ? clusters.Average(c => c.ModularityScore)
            : 0.0;

        _logger.LogInformation(
            "Louvain clustering complete: {ClusterCount} clusters, {TotalItems} content items, modularity: {Modularity:F3}",
            clusters.Count, totalMembers, overallModularity);

        return new ClusteringResultDto(
            clusters.Count,
            totalMembers,
            overallModularity,
            clusters.Select(c => new ContentClusterDto(
                c.Id, c.Label, c.CommunityId, c.ModularityScore,
                c.TopEntities, c.MemberCount)).ToList());
    }
}

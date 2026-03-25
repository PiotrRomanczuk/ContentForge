using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.ContentGraph;

// Command to run Louvain community detection across all content.
// This builds a graph from shared entities (weighted by salience),
// then finds clusters of related content using the Louvain algorithm.
// Parameterless — operates on the entire content corpus.
public record RunClusteringCommand : IRequest<ClusteringResultDto>;

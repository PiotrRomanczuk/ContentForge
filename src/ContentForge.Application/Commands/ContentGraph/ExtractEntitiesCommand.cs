using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.ContentGraph;

// MediatR command to extract named entities from a content item's text.
// Like dispatching a Redux action: send this command, the handler does the work and returns results.
public record ExtractEntitiesCommand(Guid ContentItemId) : IRequest<EntityExtractionResultDto>;

using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.RenderContent;

// MediatR command — a message that triggers a handler.
// IRequest<T> = "this command returns T". Like defining a typed event payload in an event bus.
public record RenderContentCommand(
    Guid ContentItemId,
    string? TemplateName = null,
    Dictionary<string, string>? Parameters = null
) : IRequest<RenderContentResultDto>;

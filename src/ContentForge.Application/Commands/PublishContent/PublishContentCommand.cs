using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.PublishContent;

public record PublishContentCommand(
    Guid ContentItemId,
    Guid SocialAccountId
) : IRequest<PublishContentResultDto>;

using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.ApproveContent;

public record BulkApproveCommand(
    IReadOnlyList<ApprovalDecisionDto> Decisions
) : IRequest<BulkApprovalResultDto>;

using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.ApproveContent;

// MediatR command — part of CQRS pattern. Think of it as a typed event/message.
// `: IRequest<BulkApprovalResultDto>` = "this command, when sent, returns a BulkApprovalResultDto".
// Like dispatching a Redux action that returns a result, but server-side.
// The handler (BulkApproveHandler) picks this up automatically via MediatR's DI wiring.
public record BulkApproveCommand(
    IReadOnlyList<ApprovalDecisionDto> Decisions
) : IRequest<BulkApprovalResultDto>;

using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.ManageSchedule;

public record UpdateScheduleCommand(
    Guid ScheduleConfigId,
    UpdateScheduleDto Updates
) : IRequest<ScheduleConfigDto>;

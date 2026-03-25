using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.ManageSchedule;

public record CreateScheduleCommand(CreateScheduleDto Schedule) : IRequest<ScheduleConfigDto>;

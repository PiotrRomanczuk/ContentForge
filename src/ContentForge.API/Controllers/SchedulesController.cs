using ContentForge.Application.Commands.ManageSchedule;
using ContentForge.Application.DTOs;
using ContentForge.Application.Services;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentForge.API.Controllers;

// CRUD for publishing schedules + Hangfire job status.
// Each schedule links a BotRegistration to a SocialAccount on a cron pattern.
[ApiController]
[Route("api/schedules")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleConfigRepository _scheduleRepository;
    private readonly IMediator _mediator;
    private readonly IScheduleJobManager _jobManager;
    private readonly ILogger<SchedulesController> _logger;

    public SchedulesController(
        IScheduleConfigRepository scheduleRepository,
        IMediator mediator,
        IScheduleJobManager jobManager,
        ILogger<SchedulesController> logger)
    {
        _scheduleRepository = scheduleRepository;
        _mediator = mediator;
        _jobManager = jobManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ScheduleConfigDto>>> GetAll(
        [FromQuery] bool? active = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving schedules (active filter: {Active})", active);
        var schedules = active == true
            ? await _scheduleRepository.GetActiveAsync(cancellationToken)
            : await _scheduleRepository.GetAllAsync(cancellationToken);

        return Ok(schedules.Select(s => new ScheduleConfigDto(
            s.Id, s.BotRegistrationId,
            s.BotRegistration?.BotName ?? "unknown",
            s.SocialAccountId,
            s.SocialAccount?.Name ?? "unknown",
            s.CronExpression, s.IsActive,
            s.PreferredContentType.ToString(), s.CreatedAt)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ScheduleConfigDto>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving schedule {Id}", id);
        var s = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
        if (s is null)
        {
            _logger.LogDebug("Schedule {Id} not found", id);
            return NotFound();
        }

        return Ok(new ScheduleConfigDto(
            s.Id, s.BotRegistrationId,
            s.BotRegistration?.BotName ?? "unknown",
            s.SocialAccountId,
            s.SocialAccount?.Name ?? "unknown",
            s.CronExpression, s.IsActive,
            s.PreferredContentType.ToString(), s.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<ScheduleConfigDto>> Create(
        [FromBody] CreateScheduleDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new schedule for bot '{BotId}' → account '{AccountId}'",
            dto.BotRegistrationId, dto.SocialAccountId);
        var result = await _mediator.Send(new CreateScheduleCommand(dto), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ScheduleConfigDto>> Update(
        Guid id, [FromBody] UpdateScheduleDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating schedule {Id}", id);
        var result = await _mediator.Send(
            new UpdateScheduleCommand(id, dto), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Deactivate(
        Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating schedule {Id}", id);
        await _mediator.Send(
            new UpdateScheduleCommand(id, new UpdateScheduleDto(IsActive: false)),
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get status of all Hangfire recurring jobs.
    /// </summary>
    [HttpGet("jobs")]
    public ActionResult<IReadOnlyList<JobStatusDto>> GetJobStatuses()
    {
        _logger.LogDebug("Retrieving Hangfire job statuses");
        return Ok(_jobManager.GetAllJobStatuses());
    }
}

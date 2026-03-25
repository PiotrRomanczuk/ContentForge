using ContentForge.Application.DTOs;
using ContentForge.Application.Services;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.ManageSchedule;

public class UpdateScheduleHandler : IRequestHandler<UpdateScheduleCommand, ScheduleConfigDto>
{
    private readonly IScheduleConfigRepository _scheduleRepository;
    private readonly IScheduleJobManager _jobManager;
    private readonly ILogger<UpdateScheduleHandler> _logger;

    public UpdateScheduleHandler(
        IScheduleConfigRepository scheduleRepository,
        IScheduleJobManager jobManager,
        ILogger<UpdateScheduleHandler> logger)
    {
        _scheduleRepository = scheduleRepository;
        _jobManager = jobManager;
        _logger = logger;
    }

    public async Task<ScheduleConfigDto> Handle(
        UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        var config = await _scheduleRepository.GetByIdAsync(request.ScheduleConfigId, cancellationToken)
            ?? throw new InvalidOperationException($"Schedule {request.ScheduleConfigId} not found");

        var dto = request.Updates;
        if (dto.CronExpression != null) config.CronExpression = dto.CronExpression;
        if (dto.IsActive.HasValue) config.IsActive = dto.IsActive.Value;
        if (dto.PreferredContentType != null)
            config.PreferredContentType = Enum.Parse<ContentType>(dto.PreferredContentType, ignoreCase: true);

        await _scheduleRepository.UpdateAsync(config, cancellationToken);

        // Sync with Hangfire: update or remove the recurring job
        if (config.IsActive)
            _jobManager.RegisterOrUpdateRecurringJob(config);
        else
            _jobManager.RemoveRecurringJob(config.Id);

        _logger.LogInformation("Updated schedule {Id} (active: {Active})", config.Id, config.IsActive);

        return new ScheduleConfigDto(
            config.Id, config.BotRegistrationId,
            config.BotRegistration?.BotName ?? "unknown",
            config.SocialAccountId,
            config.SocialAccount?.Name ?? "unknown",
            config.CronExpression, config.IsActive,
            config.PreferredContentType.ToString(), config.CreatedAt);
    }
}

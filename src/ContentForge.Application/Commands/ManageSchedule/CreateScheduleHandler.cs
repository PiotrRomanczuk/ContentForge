using ContentForge.Application.DTOs;
using ContentForge.Application.Services;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.ManageSchedule;

public class CreateScheduleHandler : IRequestHandler<CreateScheduleCommand, ScheduleConfigDto>
{
    private readonly IScheduleConfigRepository _scheduleRepository;
    private readonly IRepository<BotRegistration> _botRegRepository;
    private readonly ISocialAccountRepository _accountRepository;
    private readonly IScheduleJobManager _jobManager;
    private readonly ILogger<CreateScheduleHandler> _logger;

    public CreateScheduleHandler(
        IScheduleConfigRepository scheduleRepository,
        IRepository<BotRegistration> botRegRepository,
        ISocialAccountRepository accountRepository,
        IScheduleJobManager jobManager,
        ILogger<CreateScheduleHandler> logger)
    {
        _scheduleRepository = scheduleRepository;
        _botRegRepository = botRegRepository;
        _accountRepository = accountRepository;
        _jobManager = jobManager;
        _logger = logger;
    }

    public async Task<ScheduleConfigDto> Handle(
        CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Schedule;

        var botReg = await _botRegRepository.GetByIdAsync(dto.BotRegistrationId, cancellationToken)
            ?? throw new InvalidOperationException($"Bot registration {dto.BotRegistrationId} not found");

        var account = await _accountRepository.GetByIdAsync(dto.SocialAccountId, cancellationToken)
            ?? throw new InvalidOperationException($"Social account {dto.SocialAccountId} not found");

        var config = new ScheduleConfig
        {
            BotRegistrationId = dto.BotRegistrationId,
            SocialAccountId = dto.SocialAccountId,
            CronExpression = dto.CronExpression,
            IsActive = true,
            PreferredContentType = dto.PreferredContentType != null
                ? Enum.Parse<ContentType>(dto.PreferredContentType, ignoreCase: true)
                : ContentType.Image,
            OverrideConfig = dto.OverrideConfig ?? new Dictionary<string, string>()
        };

        var saved = await _scheduleRepository.AddAsync(config, cancellationToken);

        // Set navigation properties for the job manager
        saved.BotRegistration = botReg;
        saved.SocialAccount = account;

        // Register the Hangfire recurring job
        _jobManager.RegisterOrUpdateRecurringJob(saved);

        _logger.LogInformation(
            "Created schedule '{Bot}' → '{Account}' ({Cron})",
            botReg.BotName, account.Name, dto.CronExpression);

        return new ScheduleConfigDto(
            saved.Id, saved.BotRegistrationId, botReg.BotName,
            saved.SocialAccountId, account.Name, saved.CronExpression,
            saved.IsActive, saved.PreferredContentType.ToString(), saved.CreatedAt);
    }
}

using ContentForge.Application.Commands.ManageSchedule;
using FluentValidation;

namespace ContentForge.Application.Validators;

public class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleCommandValidator()
    {
        RuleFor(x => x.Schedule.BotRegistrationId)
            .NotEmpty().WithMessage("Bot registration ID is required");
        RuleFor(x => x.Schedule.SocialAccountId)
            .NotEmpty().WithMessage("Social account ID is required");
        RuleFor(x => x.Schedule.CronExpression)
            .NotEmpty().WithMessage("Cron expression is required")
            .Must(BeValidCron).WithMessage("Invalid cron expression format");
    }

    // Basic cron validation: must have 5 space-separated parts (minute hour day month weekday).
    private static bool BeValidCron(string cron)
    {
        if (string.IsNullOrWhiteSpace(cron)) return false;
        var parts = cron.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length is 5 or 6; // 5 for standard, 6 for Hangfire (with seconds)
    }
}

using ContentForge.Application.Commands.ManageSchedule;
using FluentValidation;

namespace ContentForge.Application.Validators;

public class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
{
    public UpdateScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleConfigId)
            .NotEmpty().WithMessage("Schedule config ID is required");

        // Only validate cron if provided (it's optional on updates)
        When(x => x.Updates.CronExpression != null, () =>
        {
            RuleFor(x => x.Updates.CronExpression!)
                .Must(BeValidCron).WithMessage("Invalid cron expression format");
        });
    }

    private static bool BeValidCron(string cron)
    {
        if (string.IsNullOrWhiteSpace(cron)) return false;
        var parts = cron.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length is 5 or 6;
    }
}

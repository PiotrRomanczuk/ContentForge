using ContentForge.Application.DTOs;
using FluentValidation;

namespace ContentForge.Application.Validators;

public class CreateSocialAccountValidator : AbstractValidator<CreateSocialAccountDto>
{
    public CreateSocialAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required")
            .MaximumLength(200);
        RuleFor(x => x.Platform)
            .NotEmpty().WithMessage("Platform is required")
            .Must(BeValidPlatform).WithMessage("Invalid platform. Supported: Facebook, Instagram, TikTok, YouTube");
        RuleFor(x => x.ExternalId)
            .NotEmpty().WithMessage("External ID is required")
            .MaximumLength(200);
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required");
    }

    private static bool BeValidPlatform(string platform) =>
        Enum.TryParse<Domain.Enums.Platform>(platform, ignoreCase: true, out _);
}

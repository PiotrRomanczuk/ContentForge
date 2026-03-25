using ContentForge.Application.Commands.ValidateAccount;
using FluentValidation;

namespace ContentForge.Application.Validators;

public class ValidateAccountCommandValidator : AbstractValidator<ValidateAccountCommand>
{
    public ValidateAccountCommandValidator()
    {
        RuleFor(x => x.SocialAccountId)
            .NotEmpty().WithMessage("Social account ID is required");
    }
}

using ContentForge.Application.Commands.PublishContent;
using FluentValidation;

namespace ContentForge.Application.Validators;

public class PublishContentCommandValidator : AbstractValidator<PublishContentCommand>
{
    public PublishContentCommandValidator()
    {
        RuleFor(x => x.ContentItemId)
            .NotEmpty().WithMessage("Content item ID is required");
        RuleFor(x => x.SocialAccountId)
            .NotEmpty().WithMessage("Social account ID is required");
    }
}

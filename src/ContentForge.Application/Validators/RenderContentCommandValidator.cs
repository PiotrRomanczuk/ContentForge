using ContentForge.Application.Commands.RenderContent;
using FluentValidation;

namespace ContentForge.Application.Validators;

public class RenderContentCommandValidator : AbstractValidator<RenderContentCommand>
{
    public RenderContentCommandValidator()
    {
        RuleFor(x => x.ContentItemId)
            .NotEmpty().WithMessage("Content item ID is required");
    }
}

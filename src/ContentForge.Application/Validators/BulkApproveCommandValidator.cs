using ContentForge.Application.Commands.ApproveContent;
using FluentValidation;

namespace ContentForge.Application.Validators;

// FluentValidation validator — like Zod schemas in JS/TS.
// AbstractValidator<T> = define rules for a specific type.
// MediatR's ValidationBehavior runs this automatically before the handler executes.
public class BulkApproveCommandValidator : AbstractValidator<BulkApproveCommand>
{
    public BulkApproveCommandValidator()
    {
        RuleFor(x => x.Decisions)
            .NotEmpty().WithMessage("At least one approval decision is required");

        RuleForEach(x => x.Decisions).ChildRules(decision =>
        {
            decision.RuleFor(d => d.ContentItemId)
                .NotEmpty().WithMessage("Content item ID is required for each decision");
        });
    }
}

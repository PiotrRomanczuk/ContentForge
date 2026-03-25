using ContentForge.Application.DTOs;
using ContentForge.Domain.Enums;
using FluentValidation;

namespace ContentForge.Application.Validators;

// Validates each item in an import batch before processing.
// Like a Zod schema: z.object({ botName: z.string().min(1), ... })
// Used by the controller to validate request DTOs before creating entities.
public class ImportContentItemValidator : AbstractValidator<ImportContentItemDto>
{
    public ImportContentItemValidator()
    {
        RuleFor(x => x.BotName)
            .NotEmpty().WithMessage("Bot name is required");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .Must(BeValidContentType)
            .WithMessage("Invalid content type. Valid values: Image, Carousel, Video, Story, Text");

        RuleFor(x => x.TextContent)
            .NotEmpty().WithMessage("Text content is required")
            .MaximumLength(10_000).WithMessage("Text content must not exceed 10,000 characters");
    }

    private static bool BeValidContentType(string contentType)
    {
        return Enum.TryParse<ContentType>(contentType, ignoreCase: true, out _);
    }
}

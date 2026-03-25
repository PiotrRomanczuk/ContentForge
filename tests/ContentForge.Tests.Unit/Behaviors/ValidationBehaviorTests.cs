using ContentForge.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ContentForge.Tests.Unit.Behaviors;

// The request type must be public for Moq/DynamicProxy to work with IValidator<T>.
// Alternatively we use concrete validator implementations to avoid Moq limitations.
public record TestValidationRequest(string Name) : IRequest<string>;

public class ValidationBehaviorTests
{
    // A validator that always passes — like a Zod schema that accepts everything.
    private class AlwaysPassValidator : AbstractValidator<TestValidationRequest> { }

    // A validator that always fails with specific errors.
    private class AlwaysFailValidator : AbstractValidator<TestValidationRequest>
    {
        public AlwaysFailValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters");
        }
    }

    // A validator that adds a single specific error.
    private class SingleErrorValidator : AbstractValidator<TestValidationRequest>
    {
        public SingleErrorValidator(string errorMessage)
        {
            RuleFor(x => x.Name).Must(_ => false).WithMessage(errorMessage);
        }
    }

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var validators = Enumerable.Empty<IValidator<TestValidationRequest>>();
        var behavior = new ValidationBehavior<TestValidationRequest, string>(validators);
        var request = new TestValidationRequest("test");
        var nextCalled = false;

        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be("ok");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var validator = new AlwaysPassValidator();
        var behavior = new ValidationBehavior<TestValidationRequest, string>(
            new[] { validator });
        var request = new TestValidationRequest("valid name");
        var nextCalled = false;

        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be("ok");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var validator = new AlwaysFailValidator();
        var behavior = new ValidationBehavior<TestValidationRequest, string>(
            new[] { validator });
        // Empty string triggers both NotEmpty and MinimumLength(3) rules.
        var request = new TestValidationRequest("");

        RequestHandlerDelegate<string> next = () => Task.FromResult("should not reach");

        var act = () => behavior.Handle(request, next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCountGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task Handle_MultipleValidators_AggregatesErrors()
    {
        var validator1 = new SingleErrorValidator("Error from validator 1");
        var validator2 = new SingleErrorValidator("Error from validator 2");

        // Materialize into a List to avoid double-enumeration of the IEnumerable.
        var validators = new List<IValidator<TestValidationRequest>> { validator1, validator2 };
        var behavior = new ValidationBehavior<TestValidationRequest, string>(validators);

        RequestHandlerDelegate<string> next = () => Task.FromResult("should not reach");

        var act = () => behavior.Handle(
            new TestValidationRequest("anything"), next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        // Each validator contributes 1 error, so 2 total.
        exception.Which.Errors.Should().HaveCountGreaterOrEqualTo(2);
        exception.Which.Errors.Should().Contain(e => e.ErrorMessage == "Error from validator 1");
        exception.Which.Errors.Should().Contain(e => e.ErrorMessage == "Error from validator 2");
    }
}

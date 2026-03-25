using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContentForge.Application.Behaviors;

// MediatR pipeline behavior — runs FluentValidation before every command handler.
// Like Express middleware that validates req.body before the route handler.
// IPipelineBehavior<TRequest, TResponse> = intercepts the command before the handler sees it.
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // IEnumerable<IValidator<T>> = all registered validators for this command type.
    // If no validators are registered, the enumerable is empty (no-op).
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    // Logger is optional — uses NullLogger when not provided (e.g., in unit tests).
    // NullLogger = like a no-op console.log() that swallows all output.
    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>>? logger = null)
    {
        _validators = validators;
        _logger = logger ?? NullLogger<ValidationBehavior<TRequest, TResponse>>.Instance;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var commandName = typeof(TRequest).Name;

        if (!_validators.Any())
        {
            _logger.LogDebug("No validators registered for {CommandName}", commandName);
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        // Run all validators and collect errors
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = string.Join("; ", failures.Select(f => f.ErrorMessage));
            _logger.LogWarning("Validation failed for {CommandName}: {Errors}", commandName, errors);
            throw new ValidationException(failures);
        }

        _logger.LogDebug("Validation passed for {CommandName}", commandName);
        // No validation errors — pass the command to the actual handler
        return await next();
    }
}

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs every command/query entering and leaving the pipeline.
/// Like Express middleware wrapping every route handler:
///   console.log(`→ ${commandName}`); const result = await handler(); console.log(`← ${commandName} (${ms}ms)`);
/// IPipelineBehavior = intercepts BEFORE and AFTER the handler runs.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var commandName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {CommandName}", commandName);

        // Stopwatch = like performance.now() in JS, but with nanosecond precision.
        // const start = performance.now(); ... const elapsed = performance.now() - start;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > 500)
            {
                _logger.LogWarning("Slow command: {CommandName} took {ElapsedMs}ms", commandName, elapsedMs);
            }
            else
            {
                _logger.LogInformation("Handled {CommandName} in {ElapsedMs}ms", commandName, elapsedMs);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Command {CommandName} failed after {ElapsedMs}ms",
                commandName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

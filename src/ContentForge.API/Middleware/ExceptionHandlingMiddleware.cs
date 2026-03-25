using System.Net;
using System.Text.Json;
using FluentValidation;

namespace ContentForge.API.Middleware;

/// <summary>
/// Global exception handler — like Express's error middleware: app.use((err, req, res, next) => { ... }).
/// Catches unhandled exceptions and returns consistent JSON error responses instead of stack traces.
/// </summary>
public class ExceptionHandlingMiddleware
{
    // RequestDelegate is like Express's next() — it calls the next middleware in the pipeline
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Differentiate log levels by exception type — validation errors are warnings,
            // unexpected exceptions are errors. Like using different severity levels in Winston.
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "none";
            switch (ex)
            {
                case FluentValidation.ValidationException:
                    _logger.LogWarning(ex, "Validation error for {Method} {Path} [CorrelationId: {CorrelationId}]",
                        context.Request.Method, context.Request.Path, correlationId);
                    break;
                case InvalidOperationException:
                case NotSupportedException:
                    _logger.LogWarning(ex, "Client error for {Method} {Path} [CorrelationId: {CorrelationId}]",
                        context.Request.Method, context.Request.Path, correlationId);
                    break;
                default:
                    _logger.LogError(ex, "Unhandled exception for {Method} {Path} [CorrelationId: {CorrelationId}]",
                        context.Request.Method, context.Request.Path, correlationId);
                    break;
            }
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.UnprocessableEntity,
                validationEx.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                    .ToList() as IReadOnlyList<string>
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                new List<string> { exception.Message } as IReadOnlyList<string>
            ),
            NotSupportedException => (
                HttpStatusCode.BadRequest,
                new List<string> { exception.Message } as IReadOnlyList<string>
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new List<string>
                {
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later."
                } as IReadOnlyList<string>
            ),
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        // Include CorrelationId in error responses so clients can reference it in support requests.
        var correlationId = context.Items["CorrelationId"]?.ToString();
        var response = new { status = (int)statusCode, errors, correlationId };
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

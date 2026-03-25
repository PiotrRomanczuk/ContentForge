using Serilog.Context;

namespace ContentForge.API.Middleware;

/// <summary>
/// Ensures every request has a unique CorrelationId for end-to-end tracing.
/// Like adding a requestId to every Express req object: req.id = uuid().
/// If the caller sends an X-Correlation-Id header, we reuse it (propagation).
/// Otherwise we generate a new GUID.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if caller provided a correlation ID (like forwarding a trace ID from an API gateway)
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        // Store in HttpContext.Items so other middleware/controllers can access it.
        // Like setting req.correlationId in Express middleware.
        context.Items["CorrelationId"] = correlationId;

        // Set on response header so the client can correlate their request with server logs.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(HeaderName, correlationId);
            return Task.CompletedTask;
        });

        // LogContext.PushProperty = like AsyncLocalStorage.run({ correlationId }) in Node.js.
        // Any logger used within this async scope will include the CorrelationId automatically.
        // The `using` block ensures the property is popped when the request ends (like try/finally).
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

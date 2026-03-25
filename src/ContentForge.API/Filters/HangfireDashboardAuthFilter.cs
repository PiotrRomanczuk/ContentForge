using Hangfire.Dashboard;

namespace ContentForge.API.Filters;

// Authorization filter for the Hangfire dashboard at /hangfire.
// IDashboardAuthorizationFilter = Hangfire's interface for protecting the dashboard UI.
// In development: allow all access. In production: require authentication.
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    private readonly bool _isDevelopment;

    public HangfireDashboardAuthFilter(bool isDevelopment)
    {
        _isDevelopment = isDevelopment;
    }

    public bool Authorize(DashboardContext context)
    {
        // In development, allow unrestricted access to the Hangfire dashboard
        if (_isDevelopment) return true;

        // In production, check for authenticated user (ASP.NET auth middleware)
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}

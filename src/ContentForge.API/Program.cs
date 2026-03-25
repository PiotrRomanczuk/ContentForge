using System.Text;
using ContentForge.API.Filters;
using ContentForge.API.Middleware;
using ContentForge.Application.Behaviors;
using ContentForge.Application.Commands.ApproveContent;
using ContentForge.Bots;
using ContentForge.Infrastructure;
using FluentValidation;
using ContentForge.Infrastructure.Services.Scheduling;
using Hangfire;
using Hangfire.PostgreSql;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Compact;

// Bootstrap logger — catches errors during startup before the full pipeline is ready.
// Like setting up a basic console.log() fallback before your Winston logger is configured.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{

// This is the app entry point — like index.js / server.ts in an Express app.
// .NET 8 "minimal hosting" = no Startup class, everything in one file.
// builder.Services = the DI container (like setting up providers in NestJS).
var builder = WebApplication.CreateBuilder(args);

// Replace the built-in Microsoft logger with Serilog.
// UseSerilog() = like replacing console.log with Winston/Pino in Express.
// ReadFrom.Configuration() = reads Serilog config from appsettings.json.
// Enrich.FromLogContext() = like AsyncLocalStorage — properties pushed onto LogContext
// are automatically included in every log entry within the same async scope.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ContentForge")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}  {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/contentforge-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        // CompactJsonFormatter = Serilog's structured JSON output. Each log line is a JSON object,
        // like what Pino produces in Node.js — machine-readable for log aggregation tools (ELK, Seq).
        formatter: new CompactJsonFormatter(),
        path: "logs/contentforge-json-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30));

// Register each architectural layer's services into the DI container.
// These are the extension methods defined in each layer's DependencyInjection.cs.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBots();
// Register MediatR — scans the Application assembly for all IRequestHandler implementations
// and wires them up so _mediator.Send(command) finds the right handler automatically.
// AddOpenBehavior = pipeline middleware that runs before every handler (like Express middleware).
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(BulkApproveCommand).Assembly);
    // Pipeline behaviors run in registration order (outer → inner).
    // LoggingBehavior wraps everything = captures total time including validation.
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
// Register all FluentValidation validators from the Application assembly
builder.Services.AddValidatorsFromAssembly(typeof(BulkApproveCommand).Assembly);

// JWT Authentication — like passport-jwt in Express.
// `?? throw` = nullish coalescing with an exception. Like: value ?? throwError('...')
var jwtSecret = builder.Configuration["Auth:JwtSecret"]
    ?? throw new InvalidOperationException("Auth:JwtSecret must be configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // TokenValidationParameters = what to check when a JWT arrives in the Authorization header.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Auth:Issuer"] ?? "ContentForge",
            ValidAudience = builder.Configuration["Auth:Audience"] ?? "ContentForge-API",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// AddControllers() = scan for [ApiController] classes and register their routes.
// Like app.use('/api', router) in Express, but automatic via [Route] attributes.
// JsonStringEnumConverter = serialize enums as "Facebook" instead of 0 in API responses.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
// Swagger = auto-generated API docs at /swagger. Like swagger-jsdoc + swagger-ui-express.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ContentForge API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT token. Get one from POST /auth/token",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks();

// H-5: Request body size limit — prevent DoS via huge payloads (e.g., massive TextContent on /import).
// Like express.json({ limit: '5mb' }) — rejects requests with body > 5 MB.
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 5 * 1024 * 1024; // 5 MB
});

// H-1: CORS — restrict cross-origin requests. Like cors({ origin: [...] }) in Express.
// Configurable via "Cors:AllowedOrigins" in appsettings (comma-separated).
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
    ?? new[] { "http://localhost:3000", "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// H-2: Rate limiting — prevent brute-force attacks on auth endpoint.
// Like express-rate-limit: { windowMs: 60000, max: 5 }.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// Hangfire = background job scheduler. Like node-cron or BullMQ in Node.js.
// UsePostgreSqlStorage = stores job queue in PostgreSQL (same DB, separate "hangfire" schema).
var hangfireConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(hangfireConnectionString)));
// AddHangfireServer = starts a background worker that processes queued jobs.
// Like starting a BullMQ worker alongside your Express server.
builder.Services.AddHangfireServer();

// builder.Build() = finalize DI container and create the app. After this, no more service registration.
var app = builder.Build();

// Two-phase bot init: DI container is built, now we can pull singletons and register bots.
ContentForge.Bots.DependencyInjection.InitializeBots(app.Services);

// ── Startup diagnostics ──
// Log key configuration so operators can verify the app started with the right settings.
Log.Information("ContentForge starting in {Environment} mode", app.Environment.EnvironmentName);
Log.Information("CORS allowed origins: {Origins}", string.Join(", ", allowedOrigins));
Log.Information("Hangfire storage: PostgreSQL (connection configured)");

// Middleware pipeline — order matters, just like app.use() order in Express.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global error handler — must be first to catch all exceptions downstream.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Correlation ID middleware — assigns a unique ID to every request for tracing.
// Must come before Serilog request logging so the ID is available for enrichment.
app.UseMiddleware<CorrelationIdMiddleware>();

// Serilog request logging — logs every HTTP request with method, path, status code, duration.
// Like morgan('combined') in Express, but structured and with correlation IDs.
app.UseSerilogRequestLogging(options =>
{
    // Enrich each request log entry with extra context.
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("CorrelationId",
            httpContext.Items["CorrelationId"]?.ToString() ?? "none");
    };
    // Log slow requests (>500ms) at Warning level instead of Information.
    // Like custom morgan tokens that flag slow responses.
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null) return Serilog.Events.LogEventLevel.Error;
        if (elapsed > 500) return Serilog.Events.LogEventLevel.Warning;
        if (httpContext.Response.StatusCode >= 500) return Serilog.Events.LogEventLevel.Error;
        return Serilog.Events.LogEventLevel.Information;
    };
});

// H-3: Security headers — prevent clickjacking, MIME sniffing, enforce HTTPS.
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    await next();
});

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();  // Must come before Authorization
app.UseAuthorization();
app.MapControllers();     // Maps all [Route] attributes from controllers
app.MapHealthChecks("/health");

// H-4: Hangfire dashboard — only available in development.
// In production, use the /api/schedules/jobs endpoint instead.
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireDashboardAuthFilter(isDevelopment: true) }
    });
}

// Bootstrap scheduled jobs from DB — loads active schedules and registers Hangfire recurring jobs.
await ScheduleBootstrapper.InitializeAsync(app.Services);

// Minimal API endpoint — like app.get('/', (req, res) => res.json({...})) in Express.
// `new { ... }` = anonymous object, serialized to JSON automatically.
app.MapGet("/", () => Results.Ok(new
{
    Name = "ContentForge",
    Version = "0.1.0",
    Status = "Running"
}));

// Starts the Kestrel web server. Like app.listen(8080) in Express. Blocks until shutdown.
app.Run();

}
catch (Exception ex)
{
    // Fatal = the app cannot start. Like process.on('uncaughtException') crashing the Node.js process.
    Log.Fatal(ex, "ContentForge terminated unexpectedly");
}
finally
{
    // Flush all buffered log events before the process exits.
    // Like calling winston.end() or pino.final() to ensure nothing is lost.
    Log.CloseAndFlush();
}

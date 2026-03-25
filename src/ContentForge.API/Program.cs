using System.Text;
using ContentForge.Application.Commands.ApproveContent;
using ContentForge.Bots;
using ContentForge.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// This is the app entry point — like index.js / server.ts in an Express app.
// .NET 8 "minimal hosting" = no Startup class, everything in one file.
// builder.Services = the DI container (like setting up providers in NestJS).
var builder = WebApplication.CreateBuilder(args);

// Register each architectural layer's services into the DI container.
// These are the extension methods defined in each layer's DependencyInjection.cs.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBots();
// Register MediatR — scans the Application assembly for all IRequestHandler implementations
// and wires them up so _mediator.Send(command) finds the right handler automatically.
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(BulkApproveCommand).Assembly));

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
builder.Services.AddControllers();
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

// builder.Build() = finalize DI container and create the app. After this, no more service registration.
var app = builder.Build();

// Two-phase bot init: DI container is built, now we can pull singletons and register bots.
ContentForge.Bots.DependencyInjection.InitializeBots(app.Services);

// Middleware pipeline — order matters, just like app.use() order in Express.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();  // Must come before Authorization
app.UseAuthorization();
app.MapControllers();     // Maps all [Route] attributes from controllers
app.MapHealthChecks("/health");

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

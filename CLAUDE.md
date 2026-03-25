# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

ContentForge is a multi-bot, multi-platform content management API for batch content generation, editorial review, and automated publishing to social media (Facebook, Instagram). Built with .NET 8 and Clean Architecture.

**Current state:** Phases 1-2 are implemented (domain models, JWT auth, content import, bulk approval). No EF migrations exist yet — the app cannot run against a fresh database without generating one first. Media rendering, platform publishing, scheduling, and metrics collection are modeled as interfaces but have no implementations.

## Build & Run Commands

```bash
# Build entire solution
dotnet build

# Run API locally (requires PostgreSQL at localhost:5432)
dotnet run --project src/ContentForge.API

# Run with Docker (PostgreSQL included)
docker compose up -d

# Run all tests
dotnet test

# Run only unit tests
dotnet test tests/ContentForge.Tests.Unit

# Run only integration tests
dotnet test tests/ContentForge.Tests.Integration

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Generate EF migration (run from repo root)
dotnet ef migrations add <MigrationName> --project src/ContentForge.Infrastructure --startup-project src/ContentForge.API

# Apply migrations
dotnet ef database update --project src/ContentForge.Infrastructure --startup-project src/ContentForge.API
```

## Architecture

Clean Architecture (Onion) with 5 projects. Dependencies flow inward only: API → Bots/Infrastructure → Application → Domain.

**Domain** — Zero dependencies. Entities (`ContentItem`, `SocialAccount`, `PublishRecord`, `ContentMetric`, `ScheduleConfig`, `BotRegistration`), enums (`ContentStatus`, `ContentType`, `Platform`), and interfaces (`IRepository<T>`, `IContentItemRepository`, `IBotDefinition`, `IMediaRenderer`, `IPlatformAdapter`). All entities inherit from `BaseEntity` (Id, CreatedAt, UpdatedAt).

**Application** — References Domain only. MediatR commands/handlers (CQRS), DTOs, and service interfaces (`IBotRegistry`). FluentValidation is imported but no validators exist yet. The only command so far is `BulkApproveCommand` → `BulkApproveHandler`.

**Infrastructure** — EF Core + Npgsql for PostgreSQL. `ContentForgeDbContext` with 6 DbSets. Generic `Repository<T>` base class plus `ContentItemRepository` with status/bot/scheduling queries and `BulkUpdateStatusAsync` (uses `ExecuteUpdateAsync`). `BotRegistry` is a `ConcurrentDictionary`-backed singleton. EF configurations store enums as strings and `Dictionary<string, string>` properties as `jsonb` columns.

**Bots** — Concrete `IBotDefinition` implementations (`EnglishFactsBot`, `HoroscopeBot`). Each defines prompt templates per content type and language using pattern matching. Registered at startup via `DependencyInjection.InitializeBots()` which resolves the singleton `IBotRegistry` and registers each bot.

**API** — ASP.NET Core Web API. JWT Bearer auth (HMAC-SHA256). Three controllers:
- `AuthController` — `POST /auth/token` exchanges API key for JWT
- `ContentController` — import, pending review, bulk approve, get by status/id, stats dashboard
- `BotsController` — list bots, get prompt templates

## Content Lifecycle

```
Draft → Generated → Rendered → Queued → Publishing → Published
                                                    → Failed
```

Import sets status to `Generated`. Bulk approval moves to `Queued` (approved) or back to `Draft` (rejected). `Rendered`, `Publishing`, `Published`, `Failed` transitions have no implementation yet.

## Key Patterns

- **CQRS via MediatR** — Commands in `Application/Commands/`, dispatched from controllers via `IMediator.Send()`
- **Repository pattern** — Generic `IRepository<T>` with specialized `IContentItemRepository`
- **Bot registry** — Runtime singleton, bots registered imperatively at startup (not discovered via reflection)
- **DI registration** — Each layer has its own `DependencyInjection` static class (`AddInfrastructure()`, `AddBots()`)
- **Request DTOs** — Defined inline in controller files (e.g., `ImportContentRequest` in `ContentController.cs`)

## Testing

- **Unit tests:** xUnit + FluentAssertions + Moq. Project references Application, Infrastructure, Bots.
- **Integration tests:** xUnit only. Project references API.
- Both test projects currently have only placeholder tests.

## Configuration

Environment variables or `appsettings.Development.json`:
- `ConnectionStrings__DefaultConnection` — PostgreSQL connection string
- `Auth__ApiKey` — API key for JWT token exchange
- `Auth__JwtSecret` — Must be at least 32 characters
- `Auth__Issuer`, `Auth__Audience`, `Auth__TokenExpirationHours`

Local PostgreSQL: `localhost:5432`, login `admin`, password `admin`.

Docker Compose exposes API on port 8080, PostgreSQL on 5432.

## Unimplemented Interfaces

These are defined in Domain but have no concrete implementations yet:
- `IMediaRenderer` — Image/carousel rendering (SixLabors.ImageSharp packages are imported)
- `IPlatformAdapter` — Social media publishing (Facebook, Instagram, TikTok, YouTube)

Imported but unconfigured packages: Hangfire (scheduling), MassTransit.RabbitMQ (messaging), Polly (retry policies).

## Code Style: Comments for JS Developers

The primary developer comes from a JavaScript/TypeScript background. When writing or modifying C# code, add inline comments that explain .NET/C#-specific concepts in terms a JS developer would understand. Examples:

- `services.AddScoped<IFoo, Foo>();` → explain DI lifetime (Scoped = per-request, like creating a new instance per Express request handler)
- `async Task<ActionResult<T>>` → note this is like `async (req, res) => Promise<Response>` in Express
- `IMediator.Send()` → explain it's like an event emitter / message bus dispatching to a handler
- `DbSet<T>`, `ToListAsync()` → compare to Prisma/Sequelize query builders
- `IEntityTypeConfiguration` → compare to a Mongoose schema or Prisma schema definition
- `record` types → explain as frozen/immutable objects, similar to `Object.freeze({ ... })`
- Pattern matching (`switch` expressions) → compare to destructuring + conditional logic
- `CancellationToken` → explain as similar to `AbortController.signal`

Don't over-comment obvious logic — focus on C#/.NET idioms that would be unfamiliar to someone used to Node.js/Express/React.

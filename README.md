# ContentForge

A multi-bot, multi-platform, multi-account content management platform built with .NET 8 and Clean Architecture. Designed for batch content generation, editorial review, and automated publishing to Facebook and Instagram.

## Architecture

```
ContentForge.sln
├── src/
│   ├── ContentForge.Domain          # Entities, Enums, Interfaces
│   ├── ContentForge.Application     # Use Cases, DTOs, Commands (MediatR)
│   ├── ContentForge.Infrastructure  # EF Core, Repositories, Services
│   ├── ContentForge.Bots            # Bot definitions (category templates)
│   └── ContentForge.API             # ASP.NET Core Web API, Controllers, JWT Auth
├── tests/
│   ├── ContentForge.Tests.Unit
│   └── ContentForge.Tests.Integration
├── Dockerfile
└── docker-compose.yml
```

**Design Patterns:** Clean Architecture (Onion), Repository Pattern, Mediator (MediatR), Adapter Pattern for platform integrations.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| API | ASP.NET Core 8 Web API |
| Auth | JWT Bearer (HMAC-SHA256) |
| ORM | Entity Framework Core + Npgsql |
| Database | PostgreSQL 16 |
| Messaging | MassTransit + RabbitMQ (planned) |
| Scheduler | Hangfire with PostgreSQL storage (planned) |
| Media | SixLabors.ImageSharp |
| CQRS | MediatR |
| Validation | FluentValidation |
| Tests | xUnit + FluentAssertions + Moq |
| Container | Docker + Docker Compose |
| Deploy | Google Cloud Run |

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL (or use Docker Compose)

### Local Development

```bash
# Clone the repo
git clone https://github.com/PiotrRomanczuk/ContentForge.git
cd ContentForge

# Run with Docker Compose
docker compose up -d

# Or run locally (requires PostgreSQL)
cd src/ContentForge.API
dotnet run
```

### Configuration

Set these environment variables or update `appsettings.Development.json`:

```
ConnectionStrings__DefaultConnection=Host=localhost;Database=contentforge;Username=postgres;Password=postgres
Auth__ApiKey=your-secret-api-key
Auth__JwtSecret=your-jwt-secret-at-least-32-characters
```

## API Endpoints

### Authentication

```bash
# Get a JWT token
curl -X POST http://localhost:8080/auth/token \
  -H "Content-Type: application/json" \
  -d '{"apiKey": "your-secret-api-key"}'
```

### Content Import

```bash
# Import content batch (generated externally, e.g. via Claude Code)
curl -X POST http://localhost:8080/api/content/import \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {
        "botName": "EnglishFactsBot",
        "category": "Language Learning",
        "contentType": "Image",
        "textContent": "Did you know? The word \"set\" has 430 different meanings...",
        "properties": { "language": "en" }
      }
    ]
  }'
```

### Content Management

```bash
# Get pending content for review
curl http://localhost:8080/api/content/pending \
  -H "Authorization: Bearer <token>"

# Bulk approve/reject
curl -X POST http://localhost:8080/api/content/approve \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "decisions": [
      { "contentItemId": "guid-here", "approved": true },
      { "contentItemId": "guid-here", "approved": false }
    ]
  }'

# Get content stats dashboard
curl http://localhost:8080/api/content/stats \
  -H "Authorization: Bearer <token>"
```

### Bot Definitions

```bash
# List all registered bots
curl http://localhost:8080/api/bots \
  -H "Authorization: Bearer <token>"

# Get prompt template for generating content externally
curl "http://localhost:8080/api/bots/EnglishFactsBot/prompt?contentType=Image&language=pl" \
  -H "Authorization: Bearer <token>"
```

## Content Workflow

```
1. GET /api/bots/{name}/prompt     → Get prompt template for Claude Code
2. Generate content externally      → Use Claude Code / Gemini / any AI
3. POST /api/content/import         → Import generated content
4. GET /api/content/pending         → Review in editorial session
5. POST /api/content/approve        → Approve/reject/edit batch
6. Hangfire publishes on schedule   → Auto-publish to FB/IG (planned)
7. Webhooks collect metrics         → Analytics dashboard (planned)
```

## Registered Bots

| Bot | Category | Languages | Content Types |
|-----|----------|-----------|---------------|
| EnglishFactsBot | Language Learning | PL, EN | Image, Carousel |
| HoroscopeBot | Entertainment | PL, EN | Image, Carousel |

## Roadmap

- [x] Phase 1: Clean Architecture scaffold, Domain models, EF Core
- [x] Phase 2: JWT auth, Content import endpoint, Bulk approval
- [ ] Phase 3: ImageSharp media rendering, template engine
- [ ] Phase 4: Meta Graph API adapters (Facebook, Instagram)
- [ ] Phase 5: Hangfire scheduling, auto-publishing pipeline
- [ ] Phase 6: Metrics collection, analytics dashboard
- [ ] Phase 7: React frontend (Vite) for editorial sessions
- [ ] Phase 8: Cloud Run deployment, CI/CD with GitHub Actions

## License

MIT

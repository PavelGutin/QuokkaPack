# Project Structure & Organization

## Solution Layout
QuokkaPack follows a clean architecture pattern with clear separation of concerns across multiple projects.

## Source Projects (`src/`)

### Core Application Projects
- **QuokkaPack.API** - REST API with JWT authentication and Swagger documentation
- **QuokkaPack.Razor** - Primary web frontend using Razor Pages with Entra ID integration
- **QuokkaPack.Blazor** - Alternative Blazor-based frontend (experimental)
- **QuokkaPack.Angular** - Angular frontend option (experimental)

### Shared Libraries
- **QuokkaPack.Data** - Entity Framework DbContext, models, and database migrations
- **QuokkaPack.Shared** - DTOs, models, and shared utilities across projects
- **QuokkaPack.ServerCommon** - Common server-side functionality and configurations

## Test Projects (`tests/`)
- **QuokkaPack.ApiTests** - API integration and unit tests

## Configuration Files
- **Directory.Build.props** - Global MSBuild properties (nullable reference types enabled)
- **docker-compose.yml** - Multi-container development environment
- **QuokkaPack.sln** - Visual Studio solution file

## Architecture Principles

### Dependency Flow
```
Frontend (Razor/Blazor/Angular) → API → Data Layer
                ↓
            Shared Models
```

### Project References
- Frontend projects reference `ServerCommon` and `Shared`
- API references `Data` and `ServerCommon`
- Data layer references `Shared` for models
- No circular dependencies between layers

### Naming Conventions
- All projects prefixed with `QuokkaPack.`
- Folder structure mirrors namespace structure
- Controllers, Pages, and Components follow ASP.NET Core conventions

### Docker Organization
- Each deployable project has its own Dockerfile
- Docker Compose orchestrates multi-service development
- SQL Server runs as separate container service

## Development Workflow
- Solution-level builds ensure all projects compile together
- Shared models in `QuokkaPack.Shared` maintain API contracts
- Database changes managed through EF Core migrations in `QuokkaPack.Data`
- Tests organized by project area in dedicated `tests/` folder
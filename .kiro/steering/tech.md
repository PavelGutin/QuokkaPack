# Technology Stack & Build System

## Framework & Runtime
- **.NET 9.0** - Primary framework for all projects
- **C#** with nullable reference types enabled globally
- **ASP.NET Core** for web applications and APIs

## Frontend Technologies
- **Razor Pages** - Primary web UI framework
- **Blazor** - Secondary/experimental UI framework
- **Angular** - Additional frontend option (experimental)

## Backend & Data
- **Entity Framework Core 9.0** - ORM and data access
- **SQL Server** - Primary database (with SQLite for development)
- **Microsoft Identity** - Authentication and user management
- **Microsoft Entra ID** - External identity provider
- **JWT Bearer Authentication** - API security

## Infrastructure & DevOps
- **Docker** - Containerization with multi-service compose setup
- **Serilog** - Structured logging to console and files
- **Swagger/OpenAPI** - API documentation and testing

## Key Libraries
- Microsoft.Identity.Web - Entra ID integration
- Microsoft.AspNetCore.Authentication.JwtBearer - JWT handling
- Serilog.AspNetCore - Logging framework
- Microsoft.EntityFrameworkCore.SqlServer - Database provider

## Common Commands

### Build & Run
```bash
# Build entire solution
dotnet build

# Run API project
dotnet run --project src/QuokkaPack.API

# Run Razor Pages frontend
dotnet run --project src/QuokkaPack.Razor

# Run with Docker Compose
docker-compose up --build
```

### Database Operations
```bash
# Add migration
dotnet ef migrations add <MigrationName> --project src/QuokkaPack.Data --startup-project src/QuokkaPack.API

# Update database
dotnet ef database update --project src/QuokkaPack.Data --startup-project src/QuokkaPack.API
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/QuokkaPack.ApiTests
```

## Development Environment
- **Visual Studio 2022** or **VS Code** recommended
- **Docker Desktop** required for containerized development
- **SQL Server** or **SQL Server Express** for local database
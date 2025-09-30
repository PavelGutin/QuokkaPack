# 🧳 QuokkaPack

**QuokkaPack** is a smart, user-friendly packing list application that helps you plan trips and organize what to bring - without forgetting anything important.

Whether you're traveling solo or prepping for a family adventure, QuokkaPack makes it easy to:

- 🧳 Create and manage trips  
- 🏷️ Organize gear into reusable categories  
- ✅ Track packing items per trip  
- 👤 Securely log in and save your personalized lists  
- 📦 Quickly reuse or customize packing templates

---

## ✨ Key Features

- **Trip planning made simple** - Create trips, choose categories, and start packing
- **Smart default suggestions** - Common categories preselected to get you started faster
- **Reusable categories and items** - Build your own gear library for repeat use
- **Multiple frontend options** - Choose from Razor Pages, Blazor Server, or Angular SPA
- **Fully containerized** - Deploy anywhere with Docker support
- **Self-hosting ready** - All-in-one containers for easy deployment
- **Cloud-connected** - Secure authentication with Microsoft Entra ID

---

## 🏗️ Architecture

QuokkaPack follows a clean architecture pattern with clear separation of concerns:

### 🎯 Core Components

```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│   Razor Pages   │  │  Blazor Server  │  │   Angular SPA   │
│   Frontend      │  │   Frontend      │  │   Frontend      │
└─────────┬───────┘  └─────────┬───────┘  └─────────┬───────┘
          │                    │                    │
          └────────────────────┼────────────────────┘
                               │
                    ┌─────────────────┐
                    │   REST API      │
                    │ (ASP.NET Core)  │
                    └─────────┬───────┘
                              │
                    ┌─────────────────┐
                    │   Data Layer    │
                    │ (Entity Framework)│
                    └─────────┬───────┘
                              │
          ┌───────────────────┼───────────────────┐
          │                   │                   │
    ┌───────────┐    ┌─────────────┐    ┌─────────────┐
    │SQL Server │    │   SQLite    │    │  In-Memory  │
    │(Production)│    │(Self-Host) │    │(Development)│
    └───────────┘    └─────────────┘    └─────────────┘
```

### 📦 Project Structure

| Project | Description | Technology |
|---------|-------------|------------|
| **QuokkaPack.API** | REST API with JWT authentication | ASP.NET Core 9.0, Swagger |
| **QuokkaPack.Razor** | Server-side rendered web app | Razor Pages, Bootstrap |
| **QuokkaPack.Blazor** | Interactive web app | Blazor Server, SignalR |
| **QuokkaPack.Angular** | Single-page application | Angular 18, TypeScript |
| **QuokkaPack.Data** | Data access and migrations | Entity Framework Core 9.0 |
| **QuokkaPack.Shared** | Shared models and DTOs | .NET 9.0 |
| **QuokkaPack.ServerCommon** | Common server functionality | ASP.NET Core extensions |
| **QuokkaPack.TypeGen** | TypeScript type generation | MSBuild integration |

### 🔐 Authentication & Security

- **Microsoft Entra ID** integration for secure authentication
- **JWT Bearer tokens** for API security
- **Role-based authorization** with user management
- **HTTPS enforcement** in production environments
- **CORS configuration** for cross-origin requests

### 🗄️ Database Support

- **SQL Server** - Primary database for production deployments
- **SQLite** - Lightweight database for self-hosted containers
- **In-Memory** - Fast database for development and testing
- **Automatic migrations** - Database schema updates on startup

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for containerized deployment)
- [Node.js 18+](https://nodejs.org/) (for Angular development)
- [SQL Server](https://www.microsoft.com/sql-server) or [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) (for local development)

### 🎯 Visual Studio Integration (Recommended)

The easiest way to run QuokkaPack is directly from Visual Studio:

1. **Open** `QuokkaPack.sln` in Visual Studio
2. **Select** startup configuration from dropdown:
   - **Docker Compose - Development (All Services)** - Full environment with debugging
   - **Docker Compose - Razor Only** - API + Razor Pages only
   - **Docker Compose - Blazor Only** - API + Blazor Server only  
   - **Docker Compose - Angular Only** - API + Angular SPA only
   - **Self-Host - Razor/Blazor/Angular** - All-in-one containers
3. **Press F5** to start debugging or **Ctrl+F5** to run without debugging

See **[Visual Studio Integration Guide](docs/visual-studio-integration.md)** for detailed instructions.

### 🐳 Docker Deployment (Command Line)

Alternative command-line deployment options:

#### Development Environment
```bash
# Start all services (API, Razor, Blazor, Angular, Database)
.\scripts\start-dev.ps1

# Access the applications:
# - API: http://localhost:5000/swagger
# - Razor: http://localhost:5001
# - Blazor: http://localhost:5002  
# - Angular: http://localhost:4200
```

#### Production Environment
```bash
# Start production environment with HTTPS
.\scripts\start-prod.ps1

# Access the applications:
# - API: https://localhost:5443/swagger
# - Razor: https://localhost:5444
# - Blazor: https://localhost:5445
# - Angular: https://localhost:5446
```

#### Self-Host Deployment (All-in-One)
```bash
# Build and run self-contained Razor application
docker run -d -p 8080:80 -v quokkapack-data:/app/data quokkapack-selfhost-razor

# Build and run self-contained Blazor application  
docker run -d -p 8081:80 -v quokkapack-data:/app/data quokkapack-selfhost-blazor

# Build and run self-contained Angular application
docker run -d -p 8082:80 -v quokkapack-data:/app/data quokkapack-selfhost-angular
```

### 💻 Local Development

#### 1. Clone and Setup
```bash
git clone <repository-url>
cd QuokkaPack
dotnet restore
```

#### 2. Database Setup
```bash
# Update database with latest migrations
dotnet ef database update --project src/QuokkaPack.Data --startup-project src/QuokkaPack.API
```

#### 3. Run Individual Projects
```bash
# Run API (Terminal 1)
dotnet run --project src/QuokkaPack.API

# Run Razor Pages (Terminal 2)  
dotnet run --project src/QuokkaPack.Razor

# Run Blazor Server (Terminal 3)
dotnet run --project src/QuokkaPack.Blazor

# Run Angular (Terminal 4)
cd src/QuokkaPack.Angular
npm install
npm start
```

---

## 🛠️ Management Scripts

QuokkaPack includes PowerShell scripts for easy container management:

### Building Images
```bash
# Build all Docker images
.\scripts\build-all-images.ps1

# Build specific image types
.\scripts\build-all-images.ps1 -ImageType individual
.\scripts\build-all-images.ps1 -ImageType selfhost
```

### Environment Management
```bash
# Development environment
.\scripts\start-dev.ps1          # Start development services
.\scripts\stop-dev.ps1           # Stop development services

# Production environment  
.\scripts\start-prod.ps1         # Start production services
.\scripts\stop-prod.ps1          # Stop production services
```

### Maintenance
```bash
# Container cleanup
.\scripts\cleanup-containers.ps1

# System maintenance
.\scripts\maintenance.ps1 -Task all

# Health checks
.\scripts\test-health-checks.ps1

# Validate entire system
.\scripts\validate-all.ps1
```

---

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run API tests
dotnet test tests/QuokkaPack.ApiTests

# Run container tests
.\scripts\test-containers.ps1

# Run specific container test categories
.\scripts\test-containers.ps1 -TestCategory BuildVerification
.\scripts\test-containers.ps1 -TestCategory Integration
.\scripts\test-containers.ps1 -TestCategory Performance
```

### Test Categories
- **Unit Tests** - Individual component testing
- **Integration Tests** - API and database integration
- **Container Tests** - Docker build and deployment verification
- **Performance Tests** - Container startup and resource usage

---

## 📊 Monitoring & Health Checks

### Health Check Endpoints
- **API**: `/health`, `/health/ready`, `/health/live`
- **Razor**: `/health`
- **Blazor**: `/health`  
- **Angular**: Nginx health check on `/`

### Monitoring Features
- **Structured logging** with Serilog
- **Health check UI** at `/health-ui`
- **Metrics collection** for performance monitoring
- **Container resource monitoring**

### Accessing Health Information
```bash
# Check API health
curl http://localhost:5000/health

# View health dashboard
# Navigate to http://localhost:5000/health-ui
```

---

## 🔧 Configuration

### Environment Variables

#### Development
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=localhost;Database=QuokkaPack;Trusted_Connection=true;
```

#### Production
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=QuokkaPack;User Id=sa;Password=YourPassword;
JwtSettings__Secret=YourJwtSecretKey
```

#### Self-Host
```bash
ASPNETCORE_ENVIRONMENT=Production
SelfHost__DataPath=/app/data
ConnectionStrings__DefaultConnection=Data Source=/app/data/quokkapack.db
```

### Configuration Files
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides
- `appsettings.Container.json` - Container-specific settings

---

## 📚 Documentation

Comprehensive documentation is available in the `docs/` directory:

- **[Container Deployment Guide](docs/container-deployment.md)** - Detailed deployment instructions
- **[Container Testing Guide](docs/container-testing.md)** - Testing framework documentation
- **[Health Checks & Monitoring](docs/health-checks-monitoring.md)** - Monitoring setup guide
- **[Container Troubleshooting](docs/container-troubleshooting.md)** - Common issues and solutions
- **[SQLite Configuration](docs/sqlite-configuration.md)** - Self-host database setup
- **[Quick Reference](docs/quick-reference.md)** - Command reference guide

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Workflow
1. Run `.\scripts\validate-all.ps1` to ensure everything works
2. Add tests for new functionality
3. Update documentation as needed
4. Ensure all CI/CD checks pass

---

## 📋 Current Status

- ✅ **Authentication** - Microsoft Entra ID integration
- ✅ **Multi-Frontend** - Razor, Blazor, and Angular support
- ✅ **Containerization** - Full Docker support with self-host options
- ✅ **Database** - SQL Server and SQLite support with migrations
- ✅ **Testing** - Comprehensive test suite including container tests
- ✅ **Monitoring** - Health checks and structured logging
- ✅ **Documentation** - Complete deployment and usage guides

### 🔜 Roadmap

- 🧺 Enhanced packing checklist features
- ✏️ Advanced category and item management
- 🔁 Trip duplication and template system
- 📱 Progressive Web App (PWA) support
- 🌐 Multi-language support
- 📊 Advanced analytics and reporting

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🆘 Support

- **Issues**: [GitHub Issues](../../issues)
- **Documentation**: [docs/](docs/)
- **Health Checks**: Visit `/health-ui` on any running instance
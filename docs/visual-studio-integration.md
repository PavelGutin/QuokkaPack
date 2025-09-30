# Visual Studio Integration Guide

This guide explains how to use QuokkaPack's Docker integration directly from Visual Studio.

## 🎯 Available Launch Configurations

When you open the QuokkaPack solution in Visual Studio, you'll see multiple startup options in the dropdown:

### 🐳 Docker Compose Options

#### Multi-Service Environments
- **Docker Compose - Development (All Services)** - Runs all frontends + API + Database in development mode
- **Docker Compose - Production (All Services)** - Runs all services in production configuration

#### Single Frontend Options  
- **Docker Compose - Razor Only** - API + Razor Pages + Database
- **Docker Compose - Blazor Only** - API + Blazor Server + Database
- **Docker Compose - Angular Only** - API + Angular SPA + Database

#### Self-Host Containers
- **Self-Host - Razor** - All-in-one Razor container with SQLite
- **Self-Host - Blazor** - All-in-one Blazor container with SQLite  
- **Self-Host - Angular** - All-in-one Angular container with SQLite

### 🔧 Individual Project Options

Each project also has its own launch options:
- **QuokkaPack.API** - Direct API launch or Docker container
- **QuokkaPack.Razor** - Direct Razor launch or Docker container
- **QuokkaPack.Blazor** - Direct Blazor launch or Docker container

## 🚀 How to Use

### 1. Select Launch Configuration
1. Open Visual Studio
2. Load the QuokkaPack solution
3. Click the startup project dropdown (next to the green play button)
4. Choose your desired configuration

### 2. Debug vs Release
- **Debug Mode**: Enables debugging, hot reload, and development features
- **Release Mode**: Optimized builds, production-like environment

### 3. Debugging Support

#### Docker Compose Debugging
When using Docker Compose configurations with debugging:
- Set breakpoints in your C# code
- Use Visual Studio's debugging tools
- Hot reload is supported for code changes
- Container logs appear in Visual Studio output

#### Individual Container Debugging
- Full debugging support for API and frontend projects
- Attach debugger to running containers
- Step through code running in containers

## 📊 Monitoring from Visual Studio

### Container Output
- View container logs in Visual Studio Output window
- Select "Docker" from the output dropdown
- Monitor startup, health checks, and application logs

### Health Checks
Access health check endpoints while debugging:
- API Health: `http://localhost:5000/health`
- Razor Health: `http://localhost:5001/health`
- Blazor Health: `http://localhost:5002/health`
- Health Dashboard: `http://localhost:5000/health-ui`

## 🛠️ Development Workflow

### Recommended Development Flow
1. **Start with Docker Compose - Development (All Services)**
2. **Set breakpoints** in the projects you're working on
3. **Make code changes** - hot reload will update containers
4. **Test across frontends** - all are running simultaneously
5. **Use health dashboard** to monitor service status

### Testing Individual Frontends
1. **Select single frontend configuration** (e.g., "Docker Compose - Razor Only")
2. **Focus development** on that specific frontend
3. **Faster startup** with fewer services
4. **Isolated testing** of frontend-specific features

### Self-Host Testing
1. **Select self-host configuration** for deployment testing
2. **Test complete application** in single container
3. **Verify SQLite integration** and data persistence
4. **Validate production-like behavior**

## 🔧 Configuration Files

### Launch Settings
Each configuration is defined in:
- `Properties/launchSettings.json` - Main launch configurations
- `src/*/Properties/launchSettings.json` - Individual project settings

### Docker Compose Files
- `docker-compose.dev.yml` - Development environment
- `docker-compose.prod.yml` - Production environment  
- `docker-compose.razor-only.yml` - Razor-only setup
- `docker-compose.blazor-only.yml` - Blazor-only setup
- `docker-compose.angular-only.yml` - Angular-only setup
- `docker-compose.vs.debug.yml` - Visual Studio debug overrides
- `docker-compose.vs.release.yml` - Visual Studio release overrides

## 🐛 Troubleshooting

### Common Issues

#### "Docker not found" Error
- Ensure Docker Desktop is installed and running
- Restart Visual Studio after installing Docker

#### Container Build Failures
- Check Docker Desktop has sufficient resources (4GB+ RAM recommended)
- Clear Docker cache: `docker system prune -f`
- Rebuild solution: Build → Rebuild Solution

#### Port Conflicts
- Stop other applications using ports 5000-5002, 4200, 1433
- Or modify port mappings in docker-compose files

#### Debugging Not Working
- Ensure you're using Debug configuration
- Check that debugger is attached to correct process
- Verify breakpoints are in executable code paths

### Performance Tips

#### Faster Container Startup
- Use individual frontend configurations instead of all services
- Keep Docker Desktop running between sessions
- Allocate more resources to Docker Desktop

#### Efficient Development
- Use hot reload for rapid iteration
- Keep containers running between debug sessions
- Use health checks to verify service readiness

## 📋 Quick Reference

### Keyboard Shortcuts
- **F5** - Start debugging selected configuration
- **Ctrl+F5** - Start without debugging
- **Shift+F5** - Stop debugging/containers
- **Ctrl+Shift+F5** - Restart debugging

### URLs by Configuration
| Configuration | Primary URL | Additional URLs |
|---------------|-------------|-----------------|
| Development (All) | http://localhost:5001 | API: :5000, Blazor: :5002, Angular: :4200 |
| Production (All) | https://localhost:5444 | API: :5443, Blazor: :5445, Angular: :5446 |
| Razor Only | http://localhost:5001 | API: :5000 |
| Blazor Only | http://localhost:5002 | API: :5000 |
| Angular Only | http://localhost:4200 | API: :5000 |
| Self-Host Razor | http://localhost:8080 | - |
| Self-Host Blazor | http://localhost:8081 | - |
| Self-Host Angular | http://localhost:8082 | - |

### Container Management
- **View Containers**: Docker Desktop or `docker ps`
- **View Logs**: Visual Studio Output → Docker
- **Stop All**: Shift+F5 or `docker-compose down`
- **Clean Up**: `.\scripts\cleanup-containers.ps1`

## 🎯 Best Practices

1. **Start Simple**: Begin with single frontend configurations
2. **Use Debugging**: Take advantage of Visual Studio's debugging capabilities
3. **Monitor Health**: Check health endpoints regularly
4. **Clean Up**: Stop containers when not in use to save resources
5. **Update Regularly**: Keep Docker Desktop and Visual Studio updated
6. **Resource Management**: Allocate sufficient resources to Docker Desktop
7. **Network Isolation**: Each configuration uses isolated Docker networks
8. **Data Persistence**: Self-host containers persist data in Docker volumes
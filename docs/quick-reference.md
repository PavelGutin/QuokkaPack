# QuokkaPack Container Quick Reference

This quick reference provides the most commonly used commands for managing QuokkaPack containers.

## Quick Start Commands

### Development Environment
```powershell
# Start development environment
.\scripts\start-dev.ps1

# Start with rebuild
.\scripts\start-dev.ps1 -Build

# Stop development environment
.\scripts\stop-dev.ps1
```

### Production Environment
```powershell
# Build production images
.\scripts\build-all-images.ps1 -Environment prod

# Start production environment
.\scripts\start-prod.ps1

# Stop production environment
.\scripts\stop-prod.ps1
```

### Self-Host Deployment
```powershell
# Build self-host images
.\scripts\build-all-images.ps1 -ImageType selfhost

# Run Razor self-host
docker run -d --name quokkapack -p 8080:80 -v quokkapack-data:/app/data quokkapack-selfhost-razor:latest

# Run Blazor self-host
docker run -d --name quokkapack -p 8080:80 -v quokkapack-data:/app/data quokkapack-selfhost-blazor:latest

# Run Angular self-host
docker run -d --name quokkapack -p 8080:80 -v quokkapack-data:/app/data quokkapack-selfhost-angular:latest
```

## Service URLs

### Development
- **API (Swagger)**: http://localhost:5000/swagger
- **Razor Pages**: http://localhost:5001
- **Blazor Server**: http://localhost:5002
- **Angular**: http://localhost:4200
- **SQL Server**: localhost:1433 (sa/YourStrong@Passw0rd)

### Production
- **API (Swagger)**: https://localhost:5443/swagger
- **Razor Pages**: https://localhost:5444
- **Blazor Server**: https://localhost:5445
- **Angular**: https://localhost:5446
- **SQL Server**: localhost:1434 (production credentials)

### Self-Host
- **Application**: http://localhost:8080

## Common Tasks

### Building Images
```powershell
# Build all images
.\scripts\build-all-images.ps1

# Build only development images
.\scripts\build-all-images.ps1 -Environment dev

# Build without cache
.\scripts\build-all-images.ps1 -NoCache

# Build specific image type
.\scripts\build-all-images.ps1 -ImageType individual
.\scripts\build-all-images.ps1 -ImageType selfhost
```

### Managing Services
```powershell
# Start specific services
.\scripts\start-dev.ps1 -Services "api,razor,sqlserver"

# Start in background
.\scripts\start-dev.ps1 -Detached

# View logs
.\scripts\start-dev.ps1 -Logs

# Scale production services
.\scripts\start-prod.ps1 -Scale "api=2,razor=2"
```

### Cleanup and Maintenance
```powershell
# Clean up stopped containers
.\scripts\cleanup-containers.ps1

# Clean up images
.\scripts\cleanup-containers.ps1 -Type images

# Complete cleanup (WARNING: Deletes data)
.\scripts\cleanup-containers.ps1 -Type all -Force

# Run maintenance tasks
.\scripts\maintenance.ps1

# Backup databases
.\scripts\maintenance.ps1 -Task backup

# Check system health
.\scripts\maintenance.ps1 -Task health
```

### Troubleshooting
```powershell
# Check container status
docker-compose -f docker-compose.dev.yml ps

# View logs for specific service
docker-compose -f docker-compose.dev.yml logs api

# Check resource usage
docker stats

# Test database connection
docker-compose -f docker-compose.dev.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"

# Access container shell
docker-compose -f docker-compose.dev.yml exec api bash
```

## Environment Variables

### Development (.env.dev)
```env
SA_PASSWORD=YourStrong@Passw0rd
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=QuokkaPack;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;
JwtSettings__Secret=your-super-secret-jwt-key-for-development-only
```

### Production (.env.prod)
```env
SA_PASSWORD=YourProductionPassword123!
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=QuokkaPack;User Id=sa;Password=YourProductionPassword123!;TrustServerCertificate=true;
JwtSettings__Secret=your-production-jwt-secret-key-minimum-32-characters
```

### Self-Host
```env
ASPNETCORE_ENVIRONMENT=Production
SelfHost__DataPath=/app/data
JwtSettings__Secret=auto-generated-if-not-provided
```

## Common Issues and Solutions

### Port Conflicts
```powershell
# Find process using port
netstat -ano | findstr :5000

# Kill process
taskkill /PID <PID> /F
```

### Database Issues
```powershell
# Reset database
.\scripts\stop-dev.ps1 -Volumes
.\scripts\start-dev.ps1 -Build

# Run migrations manually
docker-compose -f docker-compose.dev.yml exec api dotnet ef database update
```

### Out of Disk Space
```powershell
# Check Docker usage
docker system df

# Clean up system
.\scripts\cleanup-containers.ps1 -Type system -Prune
```

### SSL Certificate Issues (Production)
```powershell
# Generate development certificates
dotnet dev-certs https -ep ./certificates/aspnetapp.pfx -p YourCertPassword
dotnet dev-certs https --trust
```

## File Locations

### Scripts
- `scripts/build-all-images.ps1` - Build Docker images
- `scripts/start-dev.ps1` - Start development environment
- `scripts/start-prod.ps1` - Start production environment
- `scripts/stop-dev.ps1` - Stop development environment
- `scripts/stop-prod.ps1` - Stop production environment
- `scripts/cleanup-containers.ps1` - Cleanup Docker resources
- `scripts/maintenance.ps1` - Maintenance tasks

### Configuration
- `docker-compose.dev.yml` - Development configuration
- `docker-compose.prod.yml` - Production configuration
- `.env.dev` - Development environment variables
- `.env.prod` - Production environment variables

### Documentation
- `docs/container-deployment.md` - Complete deployment guide
- `docs/container-troubleshooting.md` - Troubleshooting guide
- `docs/quick-reference.md` - This quick reference

### Maintenance Files
- `maintenance/backups/` - Database backups
- `maintenance/logs/` - Rotated logs
- `maintenance/reports/` - Health and performance reports

## Emergency Procedures

### Complete Reset
```powershell
# Stop everything
.\scripts\stop-dev.ps1 -All
.\scripts\stop-prod.ps1 -Force

# Clean up everything
.\scripts\cleanup-containers.ps1 -Type all -Force
docker system prune -a --volumes -f

# Rebuild and restart
.\scripts\build-all-images.ps1 -NoCache
.\scripts\start-dev.ps1 -Build
```

### Data Recovery
```powershell
# Create emergency backup
docker-compose -f docker-compose.prod.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "BACKUP DATABASE QuokkaPack TO DISK = '/var/opt/mssql/backup/emergency_backup.bak'"

# Copy backup out of container
docker cp $(docker-compose -f docker-compose.prod.yml ps -q sqlserver):/var/opt/mssql/backup/emergency_backup.bak ./emergency_backup.bak
```

## Support

For detailed information, see:
- [Container Deployment Guide](./container-deployment.md)
- [Container Troubleshooting Guide](./container-troubleshooting.md)
- [GitHub Issues](https://github.com/your-org/quokkapack/issues)

---

*Last updated: January 2024*
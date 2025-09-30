# QuokkaPack Container Troubleshooting Guide

This guide provides detailed troubleshooting steps for common issues encountered when running QuokkaPack in Docker containers.

## Table of Contents

- [Quick Diagnostics](#quick-diagnostics)
- [Common Issues](#common-issues)
- [Service-Specific Issues](#service-specific-issues)
- [Performance Issues](#performance-issues)
- [Security Issues](#security-issues)
- [Data Issues](#data-issues)
- [Network Issues](#network-issues)
- [Emergency Procedures](#emergency-procedures)

## Quick Diagnostics

### Health Check Commands

Run these commands to quickly assess the health of your QuokkaPack deployment:

```powershell
# Check all container status
docker-compose -f docker-compose.dev.yml ps

# Check resource usage
docker stats --no-stream

# Check Docker system health
docker system df
docker system events --since 1h

# Test service endpoints
curl http://localhost:5000/health  # API
curl http://localhost:5001/health  # Razor
curl http://localhost:5002/health  # Blazor
curl http://localhost:4200/        # Angular
```

### Log Analysis

```powershell
# View recent logs for all services
docker-compose -f docker-compose.dev.yml logs --tail=50

# Follow logs in real-time
docker-compose -f docker-compose.dev.yml logs -f

# Search logs for errors
docker-compose -f docker-compose.dev.yml logs | Select-String "ERROR|FATAL|Exception"

# Export logs for analysis
docker-compose -f docker-compose.dev.yml logs --since=1h > troubleshooting-logs.txt
```

## Common Issues

### 1. Containers Won't Start

#### Symptoms
- Services show as "Exited" status
- Error messages during `docker-compose up`
- Containers restart continuously

#### Diagnostic Steps
```powershell
# Check container exit codes
docker-compose -f docker-compose.dev.yml ps

# View container logs
docker-compose -f docker-compose.dev.yml logs <service-name>

# Check for port conflicts
netstat -ano | findstr ":5000"
netstat -ano | findstr ":5001"
netstat -ano | findstr ":5002"
netstat -ano | findstr ":4200"
netstat -ano | findstr ":1433"
```

#### Solutions

**Port Conflicts:**
```powershell
# Kill process using the port
taskkill /PID <PID> /F

# Or change ports in docker-compose.yml
ports:
  - "5010:5000"  # Use different external port
```

**Missing Environment Variables:**
```powershell
# Check if .env files exist
Test-Path .env.dev
Test-Path .env.prod

# Create missing environment file
cp .env.template .env.dev
# Edit with appropriate values
```

**Image Build Issues:**
```powershell
# Rebuild images
.\scripts\build-all-images.ps1 -NoCache

# Check for build errors
docker-compose -f docker-compose.dev.yml build --no-cache
```

### 2. Database Connection Issues

#### Symptoms
- API returns 500 errors
- "Cannot connect to SQL Server" messages
- Database migration failures

#### Diagnostic Steps
```powershell
# Check SQL Server container
docker-compose -f docker-compose.dev.yml logs sqlserver

# Test database connectivity
docker-compose -f docker-compose.dev.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"

# Check connection string
docker-compose -f docker-compose.dev.yml exec api env | grep ConnectionStrings
```

#### Solutions

**SQL Server Not Ready:**
```powershell
# Wait for SQL Server to fully start (can take 30-60 seconds)
# Check logs for "SQL Server is now ready for client connections"

# Increase health check start period
healthcheck:
  start_period: 120s  # Increase from default
```

**Wrong Connection String:**
```powershell
# Verify connection string format
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=QuokkaPack;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;

# Check service name matches docker-compose.yml
services:
  sqlserver:  # This name must match connection string
```

**Database Doesn't Exist:**
```powershell
# Run migrations manually
docker-compose -f docker-compose.dev.yml exec api dotnet ef database update

# Or recreate database
.\scripts\stop-dev.ps1 -Volumes
.\scripts\start-dev.ps1 -Build
```

### 3. Frontend Not Loading

#### Symptoms
- Blank pages or loading screens
- 404 errors for static assets
- API calls failing from frontend

#### Diagnostic Steps
```powershell
# Check frontend container logs
docker-compose -f docker-compose.dev.yml logs razor
docker-compose -f docker-compose.dev.yml logs blazor
docker-compose -f docker-compose.dev.yml logs angular

# Test API connectivity from frontend container
docker-compose -f docker-compose.dev.yml exec razor curl http://api:8080/health
```

#### Solutions

**API Not Accessible:**
```powershell
# Check API base URL configuration
# In appsettings.json or environment variables
"ApiSettings": {
  "BaseUrl": "http://api:8080"  # Use container name, not localhost
}
```

**Static Assets Missing:**
```powershell
# For Angular - check build output
docker-compose -f docker-compose.dev.yml exec angular ls -la /usr/share/nginx/html

# Rebuild with fresh assets
.\scripts\build-all-images.ps1 -ImageType individual -NoCache
```

**CORS Issues:**
```powershell
# Check CORS configuration in API
# Ensure frontend URLs are allowed
"AllowedOrigins": [
  "http://localhost:5001",
  "http://localhost:5002", 
  "http://localhost:4200"
]
```

### 4. Performance Issues

#### Symptoms
- Slow response times
- High CPU or memory usage
- Containers being killed (OOMKilled)

#### Diagnostic Steps
```powershell
# Monitor resource usage
docker stats

# Check container resource limits
docker inspect <container-name> | Select-String -Pattern "Memory|Cpu"

# Analyze slow queries (SQL Server)
docker-compose -f docker-compose.dev.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT TOP 10 * FROM sys.dm_exec_query_stats ORDER BY total_elapsed_time DESC"
```

#### Solutions

**Increase Memory Limits:**
```yaml
# In docker-compose.yml
services:
  api:
    deploy:
      resources:
        limits:
          memory: 1G
        reservations:
          memory: 512M
```

**Optimize Database:**
```powershell
# Increase SQL Server memory
environment:
  - MSSQL_MEMORY_LIMIT_MB=2048

# Add database indexes
# Review Entity Framework queries for N+1 problems
```

**Enable Caching:**
```csharp
// In API Startup/Program.cs
services.AddMemoryCache();
services.AddResponseCaching();
```

### 5. SSL/HTTPS Issues (Production)

#### Symptoms
- Certificate validation errors
- "This site is not secure" warnings
- HTTPS endpoints not accessible

#### Diagnostic Steps
```powershell
# Check certificate files
Test-Path ./certificates/aspnetapp.pfx
Test-Path ./certificates/aspnetapp.crt

# Verify certificate details
openssl x509 -in ./certificates/aspnetapp.crt -text -noout

# Check container certificate mount
docker-compose -f docker-compose.prod.yml exec api ls -la /app/certificates/
```

#### Solutions

**Generate Development Certificates:**
```powershell
# Create development certificates
dotnet dev-certs https -ep ./certificates/aspnetapp.pfx -p YourCertPassword
dotnet dev-certs https --trust

# Update docker-compose.prod.yml
volumes:
  - ./certificates:/app/certificates:ro
environment:
  - ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certificates/aspnetapp.pfx
  - ASPNETCORE_Kestrel__Certificates__Default__Password=YourCertPassword
```

**Production Certificates:**
```powershell
# Use Let's Encrypt or commercial certificates
# Mount certificate files as read-only volumes
volumes:
  - /etc/letsencrypt/live/yourdomain.com:/app/certificates:ro
```

## Service-Specific Issues

### API Service Issues

#### Common Problems
- Swagger UI not loading
- Authentication failures
- Database migration errors

#### Solutions
```powershell
# Enable Swagger in production (if needed)
environment:
  - ASPNETCORE_ENVIRONMENT=Development  # Temporarily

# Check JWT configuration
environment:
  - JwtSettings__Secret=your-secret-key-minimum-32-characters
  - JwtSettings__Issuer=QuokkaPack
  - JwtSettings__Audience=QuokkaPack

# Manual migration
docker-compose -f docker-compose.dev.yml exec api dotnet ef database update --verbose
```

### Razor Pages Issues

#### Common Problems
- Views not updating
- Static files not loading
- Authentication redirects failing

#### Solutions
```powershell
# Clear view cache
docker-compose -f docker-compose.dev.yml restart razor

# Check static file configuration
# Ensure wwwroot is properly copied in Dockerfile
COPY ["src/QuokkaPack.Razor/wwwroot", "wwwroot/"]

# Fix authentication URLs
"Authentication": {
  "Microsoft": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "CallbackPath": "/signin-microsoft"
  }
}
```

### Blazor Server Issues

#### Common Problems
- SignalR connection failures
- Circuit disconnections
- Real-time updates not working

#### Solutions
```powershell
# Check SignalR configuration
# Ensure proper hub configuration in Program.cs
app.MapHub<BlazorHub>("/blazorhub");

# WebSocket support
# Ensure nginx (if used) supports WebSocket upgrades
proxy_set_header Upgrade $http_upgrade;
proxy_set_header Connection "upgrade";

# Increase circuit timeout
services.AddServerSideBlazor(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
});
```

### Angular Issues

#### Common Problems
- Build failures
- Runtime errors
- API proxy not working

#### Solutions
```powershell
# Check Node.js version compatibility
docker-compose -f docker-compose.dev.yml exec angular node --version
docker-compose -f docker-compose.dev.yml exec angular npm --version

# Clear npm cache
docker-compose -f docker-compose.dev.yml exec angular npm cache clean --force

# Fix proxy configuration
# In proxy.conf.json
{
  "/api/*": {
    "target": "http://api:8080",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```

### SQL Server Issues

#### Common Problems
- Container won't start
- Out of memory errors
- Slow query performance

#### Solutions
```powershell
# Check SQL Server logs
docker-compose -f docker-compose.dev.yml logs sqlserver

# Increase memory allocation
environment:
  - MSSQL_MEMORY_LIMIT_MB=2048

# Check disk space
docker system df
docker volume ls

# Optimize database
docker-compose -f docker-compose.dev.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "DBCC CHECKDB('QuokkaPack')"
```

## Performance Issues

### Slow Container Startup

#### Causes and Solutions

**Large Images:**
```powershell
# Check image sizes
docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"

# Optimize Dockerfiles
# Use multi-stage builds
# Use .dockerignore effectively
# Choose smaller base images
```

**Resource Constraints:**
```powershell
# Increase Docker Desktop resources
# Settings → Resources → Advanced
# CPU: 4+ cores
# Memory: 8GB+
# Disk: 64GB+

# Check system resources
Get-WmiObject -Class Win32_ComputerSystem | Select-Object TotalPhysicalMemory
```

**Health Check Delays:**
```yaml
# Optimize health checks
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 5s
  retries: 3
  start_period: 30s  # Reduce if application starts quickly
```

### High Memory Usage

#### Monitoring and Solutions

```powershell
# Monitor memory usage
docker stats --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"

# Set memory limits
services:
  api:
    deploy:
      resources:
        limits:
          memory: 512M
        reservations:
          memory: 256M

# Optimize .NET applications
environment:
  - DOTNET_GCServer=1
  - DOTNET_GCConcurrent=1
  - DOTNET_GCRetainVM=1
```

### Database Performance

#### Query Optimization

```powershell
# Enable query logging
environment:
  - Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command=Information

# Analyze slow queries
docker-compose -f docker-compose.dev.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "
SELECT TOP 10 
    qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time,
    qs.execution_count,
    SUBSTRING(qt.text, qs.statement_start_offset/2+1, 
        (CASE WHEN qs.statement_end_offset = -1 
         THEN LEN(CONVERT(nvarchar(max), qt.text)) * 2 
         ELSE qs.statement_end_offset END - qs.statement_start_offset)/2) AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS qt
ORDER BY avg_elapsed_time DESC"
```

#### Index Optimization

```sql
-- Add missing indexes
CREATE INDEX IX_Trips_UserId ON Trips(UserId);
CREATE INDEX IX_Items_CategoryId ON Items(CategoryId);

-- Update statistics
UPDATE STATISTICS Trips;
UPDATE STATISTICS Categories;
UPDATE STATISTICS Items;
```

## Security Issues

### Container Security

#### Vulnerability Scanning

```powershell
# Scan images for vulnerabilities
docker scout cves quokkapack-api:latest
docker scout recommendations quokkapack-api:latest

# Update base images regularly
.\scripts\build-all-images.ps1 -NoCache
```

#### Access Control

```powershell
# Run containers as non-root user
# In Dockerfile:
RUN addgroup --system --gid 1001 appgroup
RUN adduser --system --uid 1001 appuser
USER appuser

# Use read-only root filesystem
services:
  api:
    read_only: true
    tmpfs:
      - /tmp
      - /var/tmp
```

### Network Security

#### Secure Communication

```yaml
# Use internal networks
networks:
  frontend:
    driver: bridge
  backend:
    driver: bridge
    internal: true  # No external access

services:
  api:
    networks:
      - frontend
      - backend
  sqlserver:
    networks:
      - backend  # Only backend access
```

#### Secrets Management

```yaml
# Use Docker secrets
secrets:
  db_password:
    file: ./secrets/db_password.txt
  jwt_secret:
    file: ./secrets/jwt_secret.txt

services:
  api:
    secrets:
      - db_password
      - jwt_secret
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=QuokkaPack;User Id=sa;Password_File=/run/secrets/db_password;
```

## Data Issues

### Database Corruption

#### Detection and Recovery

```powershell
# Check database integrity
docker-compose -f docker-compose.dev.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "DBCC CHECKDB('QuokkaPack')"

# Repair minor corruption
docker-compose -f docker-compose.dev.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "DBCC CHECKDB('QuokkaPack', REPAIR_FAST)"

# Restore from backup
.\scripts\backup-db.sh restore backup_20240115.bak
```

### Data Loss Prevention

#### Backup Strategies

```powershell
# Automated backups
# Create backup script
$backupScript = @"
#!/bin/bash
BACKUP_DIR="/backups"
TIMESTAMP=`$(date +%Y%m%d_%H%M%S)
docker-compose -f docker-compose.prod.yml exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P `$SA_PASSWORD -Q "BACKUP DATABASE QuokkaPack TO DISK = '/var/opt/mssql/backup/QuokkaPack_`$TIMESTAMP.bak'"
"@

$backupScript | Out-File -FilePath "scripts/automated-backup.sh" -Encoding UTF8

# Schedule with Windows Task Scheduler or cron
```

#### Volume Management

```powershell
# List volumes
docker volume ls

# Backup volume data
docker run --rm -v quokkapack_sql_data:/data -v ${PWD}/backups:/backup alpine tar czf /backup/sql_data_backup.tar.gz -C /data .

# Restore volume data
docker run --rm -v quokkapack_sql_data:/data -v ${PWD}/backups:/backup alpine tar xzf /backup/sql_data_backup.tar.gz -C /data
```

## Network Issues

### DNS Resolution

#### Container-to-Container Communication

```powershell
# Test DNS resolution
docker-compose -f docker-compose.dev.yml exec api nslookup sqlserver
docker-compose -f docker-compose.dev.yml exec razor ping api

# Check network configuration
docker network inspect quokkapack-dev_default

# Verify service names match docker-compose.yml
services:
  api:        # Use 'api' in connection strings
  sqlserver:  # Use 'sqlserver' in connection strings
```

### Port Conflicts

#### Resolution Strategies

```powershell
# Find conflicting processes
netstat -ano | findstr ":5000"
Get-Process -Id <PID>

# Use different ports
# In docker-compose.yml
ports:
  - "5010:5000"  # External:Internal
  - "5011:5001"
  - "5012:5002"
  - "4201:4200"
```

### Firewall Issues

#### Windows Firewall

```powershell
# Check firewall rules
Get-NetFirewallRule -DisplayName "*Docker*"

# Allow Docker through firewall
New-NetFirewallRule -DisplayName "Docker Desktop" -Direction Inbound -Protocol TCP -LocalPort 2375,2376 -Action Allow

# Allow application ports
New-NetFirewallRule -DisplayName "QuokkaPack Dev" -Direction Inbound -Protocol TCP -LocalPort 5000,5001,5002,4200,1433 -Action Allow
```

## Emergency Procedures

### Complete System Reset

When all else fails, perform a complete reset:

```powershell
# 1. Stop all services
.\scripts\stop-dev.ps1 -All
.\scripts\stop-prod.ps1 -Force

# 2. Clean up everything
.\scripts\cleanup-containers.ps1 -Type all -Force

# 3. System-wide Docker cleanup
docker system prune -a --volumes -f

# 4. Rebuild from scratch
.\scripts\build-all-images.ps1 -NoCache

# 5. Start fresh environment
.\scripts\start-dev.ps1 -Build
```

### Data Recovery

#### Emergency Database Recovery

```powershell
# 1. Stop application services (keep database running)
docker-compose -f docker-compose.prod.yml stop api razor blazor angular

# 2. Create emergency backup
docker-compose -f docker-compose.prod.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "BACKUP DATABASE QuokkaPack TO DISK = '/var/opt/mssql/backup/emergency_backup.bak'"

# 3. Copy backup out of container
docker cp $(docker-compose -f docker-compose.prod.yml ps -q sqlserver):/var/opt/mssql/backup/emergency_backup.bak ./emergency_backup.bak

# 4. Restore to new environment if needed
# Create new database container
# Restore backup: RESTORE DATABASE QuokkaPack FROM DISK = '/var/opt/mssql/backup/emergency_backup.bak'
```

### Service Isolation

#### Run Individual Services

```powershell
# Run only database and API for debugging
docker-compose -f docker-compose.dev.yml up sqlserver api

# Run specific frontend with external API
docker run -p 5001:5001 -e ApiSettings__BaseUrl=https://production-api.com quokkapack-razor:dev

# Debug with external database
docker run -p 5000:5000 -e ConnectionStrings__DefaultConnection="Server=external-db;..." quokkapack-api:dev
```

### Log Collection for Support

#### Comprehensive Log Collection

```powershell
# Create support package
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$supportDir = "support_logs_$timestamp"
New-Item -ItemType Directory -Path $supportDir

# Collect system information
docker version > "$supportDir/docker_version.txt"
docker-compose version > "$supportDir/compose_version.txt"
docker system df > "$supportDir/system_usage.txt"
docker system info > "$supportDir/system_info.txt"

# Collect container information
docker-compose -f docker-compose.dev.yml ps > "$supportDir/container_status.txt"
docker-compose -f docker-compose.dev.yml config > "$supportDir/compose_config.txt"

# Collect logs
docker-compose -f docker-compose.dev.yml logs --since=24h > "$supportDir/application_logs.txt"
docker events --since=24h > "$supportDir/docker_events.txt"

# Collect configuration
Copy-Item .env.dev "$supportDir/env_dev.txt" -ErrorAction SilentlyContinue
Copy-Item .env.prod "$supportDir/env_prod.txt" -ErrorAction SilentlyContinue
Copy-Item docker-compose.dev.yml "$supportDir/"
Copy-Item docker-compose.prod.yml "$supportDir/"

# Create archive
Compress-Archive -Path $supportDir -DestinationPath "$supportDir.zip"
Write-Host "Support package created: $supportDir.zip"
```

---

## Getting Help

### Before Contacting Support

1. **Check this troubleshooting guide** for your specific issue
2. **Collect logs** using the log collection script above
3. **Document the exact steps** that led to the issue
4. **Note your environment** (Windows version, Docker version, etc.)
5. **Try the emergency procedures** if appropriate

### Support Channels

- **GitHub Issues**: [QuokkaPack Issues](https://github.com/your-org/quokkapack/issues)
- **Documentation**: [Container Deployment Guide](./container-deployment.md)
- **Community**: Docker Community Forums

### Information to Include

When reporting issues, include:
- QuokkaPack version
- Docker and Docker Compose versions
- Operating system and version
- Complete error messages
- Steps to reproduce
- Support log package (if possible)

---

*Last updated: January 2024*
*Version: 1.0*
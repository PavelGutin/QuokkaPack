# Production Deployment Guide

This guide covers deploying QuokkaPack in a production environment using Docker Compose.

## Prerequisites

- Docker Engine 20.10+
- Docker Compose 2.0+
- At least 4GB RAM available for containers
- 10GB+ disk space for database and backups

## Quick Start

1. **Configure Environment Variables**
   ```powershell
   # Copy the production environment template
   Copy-Item .env.prod .env.prod.local
   
   # Edit .env.prod.local with your production values
   notepad .env.prod.local
   ```

2. **Start Production Environment**
   ```powershell
   .\scripts\start-prod.ps1 -Build
   ```

3. **Verify Deployment**
   ```powershell
   # Check service health
   docker-compose --env-file .env.prod.local -f docker-compose.prod.yml ps
   
   # View logs
   .\scripts\start-prod.ps1 -Logs
   ```

## Configuration

### Environment Variables

Edit `.env.prod.local` with your production settings:

```bash
# JWT Secret - Generate a secure 32+ character secret
JWT_SECRET=YourSecureProductionJwtSecretKeyHere32Chars

# SQL Server SA Password - Use a strong password
SA_PASSWORD=YourStrongProductionPassword123!
```

### Resource Limits

The production configuration includes resource limits:

- **API Service**: 1 CPU, 512MB RAM
- **Razor Service**: 1 CPU, 512MB RAM  
- **Blazor Service**: 1 CPU, 512MB RAM
- **Angular Service**: 0.5 CPU, 256MB RAM
- **SQL Server**: 2 CPU, 2GB RAM

Adjust these in `docker-compose.prod.yml` based on your server capacity.

## Services

### Web Applications

- **API**: http://localhost:7100 - REST API with Swagger documentation
- **Razor**: http://localhost:7200 - Primary web interface
- **Blazor**: http://localhost:7300 - Blazor Server interface
- **Angular**: http://localhost:7400 - Angular SPA interface

### Database

- **SQL Server**: localhost:1433
- **Database**: QuokkaPackDb
- **User**: sa
- **Password**: As configured in `.env.prod.local`

## Data Persistence

### Database Storage

Production data is stored in:
- `./data/prod/sql/data/` - Database files
- `./data/prod/sql/log/` - Transaction logs
- `./data/prod/sql/backup/` - Automated backups

### Backup Strategy

- **Automated Backups**: Daily at midnight
- **Retention**: 7 days
- **Location**: `./data/prod/sql/backup/`
- **Format**: Full database backup with compression

### Manual Backup

```powershell
# Create immediate backup
docker-compose --env-file .env.prod.local -f docker-compose.prod.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "BACKUP DATABASE QuokkaPackDb TO DISK = '/var/opt/mssql/backup/manual_backup.bak' WITH FORMAT, INIT"
```

## Health Monitoring

### Health Checks

All services include health checks:
- **Interval**: 30 seconds
- **Timeout**: 10 seconds
- **Retries**: 3
- **Start Period**: 60 seconds (120s for SQL Server)

### Monitoring Commands

```powershell
# Check all service health
docker-compose --env-file .env.prod.local -f docker-compose.prod.yml ps

# View specific service logs
.\scripts\start-prod.ps1 -Logs -Service quokkapack.api

# Monitor resource usage
docker stats
```

## Scaling

### Horizontal Scaling

To run multiple instances of a service:

```powershell
# Scale API service to 3 instances
docker-compose --env-file .env.prod.local -f docker-compose.prod.yml up -d --scale quokkapack.api=3
```

### Load Balancing

For production load balancing, consider:
- nginx reverse proxy
- Docker Swarm mode
- Kubernetes deployment

## Security

### Network Security

- Services communicate on isolated Docker network
- Only necessary ports exposed to host
- Database not directly accessible from outside

### Application Security

- Production logging levels (Information/Warning)
- Secure JWT token configuration
- SQL Server authentication required
- No development tools in production images

## Troubleshooting

### Common Issues

1. **Services won't start**
   ```powershell
   # Check logs for errors
   .\scripts\start-prod.ps1 -Logs
   
   # Verify environment variables
   Get-Content .env.prod.local
   ```

2. **Database connection issues**
   ```powershell
   # Check SQL Server health
   docker-compose --env-file .env.prod.local -f docker-compose.prod.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "SELECT 1"
   ```

3. **Out of disk space**
   ```powershell
   # Clean up old Docker images
   docker system prune -a
   
   # Check backup disk usage
   Get-ChildItem -Path "data/prod/sql/backup" -Recurse | Measure-Object -Property Length -Sum
   ```

### Log Locations

- **Application Logs**: Container stdout/stderr (via `docker logs`)
- **Database Logs**: SQL Server error log in container
- **Backup Logs**: Backup service container logs

## Maintenance

### Updates

1. **Pull latest code**
   ```powershell
   git pull origin main
   ```

2. **Rebuild and restart**
   ```powershell
   .\scripts\start-prod.ps1 -Build
   ```

3. **Verify health**
   ```powershell
   docker-compose --env-file .env.prod.local -f docker-compose.prod.yml ps
   ```

### Cleanup

```powershell
# Stop environment
.\scripts\stop-prod.ps1

# Remove old images
.\scripts\stop-prod.ps1 -RemoveImages

# Remove all data (DESTRUCTIVE!)
.\scripts\stop-prod.ps1 -RemoveVolumes
```

## Performance Tuning

### Database Optimization

- Monitor query performance via SQL Server logs
- Consider index optimization for large datasets
- Adjust memory allocation in `docker-compose.prod.yml`

### Application Optimization

- Monitor container resource usage with `docker stats`
- Adjust CPU/memory limits based on actual usage
- Consider caching strategies for high-traffic scenarios

## Support

For issues with production deployment:
1. Check service logs: `.\scripts\start-prod.ps1 -Logs`
2. Verify configuration: `docker-compose -f docker-compose.prod.yml config`
3. Review this documentation
4. Check Docker and system resources
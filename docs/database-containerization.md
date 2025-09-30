# Database Containerization Enhancements

This document describes the enhanced database containerization features implemented for QuokkaPack.

## Overview

The database containerization enhancements provide:

- **Automatic database initialization** with Entity Framework migrations
- **Retry logic** for database connections during startup
- **Comprehensive health checks** for monitoring database status
- **Enhanced backup and restore** capabilities
- **Persistent storage** configuration for data safety
- **Development data seeding** for easier local development

## Components

### 1. Database Initialization Service

**Location**: `src/QuokkaPack.ServerCommon/Services/DatabaseInitializationService.cs`

This hosted service automatically:
- Tests database connectivity with exponential backoff retry logic
- Creates the database if it doesn't exist
- Applies pending Entity Framework migrations
- Seeds development data in development environments

**Configuration**: Automatically registered in API applications using `AddDatabaseInitialization()` extension method.

### 2. Enhanced Docker Compose Configurations

#### Development Environment (`docker-compose.dev.yml`)
- SQL Server Developer edition with debugging support
- Volume mounts for persistent data, logs, and backups
- Automatic database initialization script execution
- Relaxed health checks suitable for development
- Hot reload support for application code

#### Production Environment (`docker-compose.prod.yml`)
- SQL Server Express edition optimized for production
- Resource limits and constraints
- Strict health checks with proper restart policies
- Automated backup service with configurable schedule
- Comprehensive logging configuration

### 3. Database Management Scripts

#### PowerShell Management Script (`scripts/manage-database.ps1`)

Provides comprehensive database management capabilities:

```powershell
# Backup database
.\scripts\manage-database.ps1 -Action backup -Environment dev

# Restore from backup
.\scripts\manage-database.ps1 -Action restore -Environment prod -BackupFile "QuokkaPackDb_20241201_120000.bak"

# Run migrations
.\scripts\manage-database.ps1 -Action migrate -Environment dev

# Check health
.\scripts\manage-database.ps1 -Action health -Environment prod

# Reset database (with confirmation)
.\scripts\manage-database.ps1 -Action reset -Environment dev -Force
```

#### Health Check Script (`scripts/database-health-check.sh`)

Performs comprehensive database health validation:
- Basic connectivity tests
- Database status and accessibility
- Schema validation and table counts
- Entity Framework migration status
- Performance metrics and blocking processes
- Memory usage monitoring

#### Enhanced Backup Script (`scripts/backup-db.sh`)

Features:
- Retry logic for connection failures
- Backup integrity verification
- Automatic cleanup of old backups
- Configurable retention policies
- Detailed logging and status reporting

### 4. Database Initialization Scripts

#### Migration Script (`scripts/init-db-with-migrations.sh`)
- Waits for SQL Server to be ready
- Creates database if it doesn't exist
- Runs Entity Framework migrations with retry logic
- Handles both containerized and standalone deployments

#### Development Initialization (`scripts/init-dev-db.sql`)
- Creates development database with appropriate settings
- Enables snapshot isolation for better concurrency
- Sets up development-friendly configurations

#### Production Initialization (`scripts/init-prod-db.sql`)
- Creates production database with optimized settings
- Configures full recovery model for backup support
- Sets up backup devices and maintenance plans

## Usage

### Starting the Environment

**Development**:
```bash
docker-compose -f docker-compose.dev.yml up -d
```

**Production**:
```bash
# Set required environment variables
export SA_PASSWORD="YourSecurePassword123!"
export JWT_SECRET="YourJWTSecretKey"

docker-compose -f docker-compose.prod.yml up -d
```

### Health Monitoring

Check database health:
```bash
# Using PowerShell script
.\scripts\manage-database.ps1 -Action health -Environment dev

# Using health check script directly
docker-compose -f docker-compose.dev.yml exec sqlserver /scripts/database-health-check.sh
```

### Backup and Restore

**Create Backup**:
```bash
# Automated via PowerShell
.\scripts\manage-database.ps1 -Action backup -Environment prod

# Manual backup
docker-compose -f docker-compose.prod.yml exec sqlserver /scripts/backup-db.sh
```

**Restore Database**:
```bash
.\scripts\manage-database.ps1 -Action restore -Environment prod -BackupFile "backup_file.bak"
```

### Migration Management

**Apply Migrations**:
```bash
# Using management script
.\scripts\manage-database.ps1 -Action migrate -Environment dev

# Direct EF Core command
docker-compose -f docker-compose.dev.yml exec quokkapack.api dotnet ef database update
```

## Configuration

### Environment Variables

**Development**:
- `SA_PASSWORD`: SQL Server SA password (default: "YourStrongPassword123!")
- `JWT_SECRET`: JWT signing secret
- `ASPNETCORE_ENVIRONMENT`: Set to "Development"

**Production**:
- `SA_PASSWORD`: SQL Server SA password (required)
- `JWT_SECRET`: JWT signing secret (required)
- `BACKUP_SCHEDULE`: Backup interval in seconds (default: 86400)
- `RETENTION_DAYS`: Backup retention period (default: 7)

### Volume Configuration

**Development Volumes**:
- `./data/dev/sql/data`: Database files
- `./data/dev/sql/log`: Transaction log files
- `./data/dev/sql/backup`: Backup files

**Production Volumes**:
- `./data/prod/sql/data`: Database files
- `./data/prod/sql/log`: Transaction log files
- `./data/prod/sql/backup`: Backup files

## Health Checks

### Container Health Checks

All database containers include comprehensive health checks:
- **Interval**: 30 seconds
- **Timeout**: 10 seconds
- **Retries**: 5-10 (depending on environment)
- **Start Period**: 60-120 seconds

### Application Health Checks

The API includes database health checks accessible at `/health`:
- Tests database connectivity
- Validates Entity Framework context
- Reports overall application health status

## Troubleshooting

### Common Issues

**Database Connection Failures**:
1. Check if SQL Server container is running: `docker-compose ps`
2. Verify health check status: `docker-compose ps --format "table {{.Service}}\t{{.Status}}"`
3. Check container logs: `docker-compose logs sqlserver`
4. Run health check script: `.\scripts\manage-database.ps1 -Action health`

**Migration Failures**:
1. Ensure database is accessible
2. Check for schema conflicts
3. Review migration history: `dotnet ef migrations list`
4. Reset database if necessary: `.\scripts\manage-database.ps1 -Action reset -Force`

**Backup/Restore Issues**:
1. Verify backup file exists and is accessible
2. Check disk space in backup volume
3. Ensure SA password is correct
4. Review backup script logs

### Monitoring and Logs

**View Container Logs**:
```bash
# SQL Server logs
docker-compose -f docker-compose.prod.yml logs -f sqlserver

# Backup service logs
docker-compose -f docker-compose.prod.yml logs -f backup

# API application logs
docker-compose -f docker-compose.prod.yml logs -f quokkapack.api
```

**Database Performance Monitoring**:
```sql
-- Active connections
SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1;

-- Blocking processes
SELECT * FROM sys.dm_exec_requests WHERE blocking_session_id > 0;

-- Database size
SELECT 
    DB_NAME() as DatabaseName,
    SUM(CASE WHEN type = 0 THEN size END) * 8 / 1024 as DataSizeMB,
    SUM(CASE WHEN type = 1 THEN size END) * 8 / 1024 as LogSizeMB
FROM sys.master_files WHERE database_id = DB_ID();
```

## Security Considerations

1. **Password Management**: Use strong passwords and consider using Docker secrets for production
2. **Network Security**: Database containers are only accessible within the Docker network
3. **Backup Security**: Backup files contain sensitive data and should be secured appropriately
4. **Access Control**: SQL Server uses SA authentication; consider implementing additional user accounts for production

## Performance Optimization

1. **Memory Limits**: Production configuration includes memory limits to prevent resource exhaustion
2. **Connection Pooling**: Entity Framework connection pooling is configured for optimal performance
3. **Backup Compression**: Backups use compression to reduce storage requirements
4. **Index Maintenance**: Consider implementing automated index maintenance for production workloads
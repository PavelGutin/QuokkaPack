# SQLite Database Configuration for Self-Hosting

This document describes the SQLite database configuration implemented for QuokkaPack self-hosting scenarios.

## Overview

QuokkaPack now supports SQLite as an alternative database provider, specifically designed for self-hosting scenarios where a full SQL Server instance is not required or desired. The SQLite configuration provides:

- Automatic database creation and migration
- Optimized performance settings
- Data persistence with proper file path management
- Health monitoring and diagnostics
- Seamless switching between SQL Server and SQLite

## Configuration

### Environment Detection

The system automatically detects when to use SQLite based on:

1. **Connection String Pattern**: If the connection string contains `Data Source=` and `.db` or `.sqlite`
2. **Self-Host Environment Variable**: `SELFHOST_DATA_PATH` is set
3. **Configuration Setting**: `SelfHost:DataPath` is configured

### Connection String Examples

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/app/data/quokkapack.db"
  }
}
```

For self-host containers:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/app/data/quokkapack.db"
  },
  "SelfHost": {
    "DataPath": "/app/data",
    "EnableAutoSeeding": true
  }
}
```

### Environment Variables

- `SELFHOST_DATA_PATH`: Specifies the directory for database and log files
- `ASPNETCORE_ENVIRONMENT`: Set to `SelfHost` for self-hosting scenarios

## Features

### Automatic Database Initialization

The `SQLiteDatabaseService` handles:

- **Directory Creation**: Ensures database and logs directories exist
- **Performance Optimization**: Applies SQLite-specific PRAGMA settings
- **Migration Application**: Runs Entity Framework migrations automatically
- **Integrity Verification**: Checks database integrity on startup

### Performance Optimizations

The following SQLite optimizations are automatically applied:

```sql
PRAGMA foreign_keys = ON;           -- Enable foreign key constraints
PRAGMA journal_mode = WAL;          -- Write-Ahead Logging for better concurrency
PRAGMA synchronous = NORMAL;        -- Balance between safety and performance
PRAGMA cache_size = -64000;         -- 64MB cache size
PRAGMA temp_store = MEMORY;         -- Store temporary data in memory
PRAGMA mmap_size = 268435456;       -- 256MB memory-mapped I/O
```

### Health Monitoring

SQLite databases include enhanced health checks that monitor:

- Database file accessibility
- Integrity verification
- Foreign key constraint validation
- Performance metrics (file size, page count, etc.)
- Configuration validation (journal mode, foreign keys)

### Data Persistence

The SQLite configuration ensures proper data persistence by:

- Creating database files in specified data directories
- Setting up log file rotation in the data directory
- Providing backup functionality
- Managing file permissions appropriately

## Usage

### Development Testing

Use the provided test script to verify SQLite configuration:

```powershell
./scripts/test-sqlite-config.ps1 -DataPath "./test-data" -Port 5000
```

### Self-Host Container

When running in a self-host container, mount a volume for data persistence:

```bash
docker run -p 8080:80 -v ./quokkapack-data:/app/data quokkapack-selfhost
```

### Manual Configuration

To manually configure SQLite for development:

1. Update `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./dev-data/quokkapack.db"
  }
}
```

2. Set environment variable:
```bash
export SELFHOST_DATA_PATH="./dev-data"
```

3. Run the application:
```bash
dotnet run --project src/QuokkaPack.API
```

## File Structure

When using SQLite, the following file structure is created:

```
{DataPath}/
├── quokkapack.db           # Main SQLite database file
├── quokkapack.db-wal       # Write-Ahead Log file
├── quokkapack.db-shm       # Shared memory file
└── logs/                   # Application logs
    ├── log-20240101.txt
    ├── log-20240102.txt
    └── ...
```

## Troubleshooting

### Common Issues

1. **Permission Errors**: Ensure the application has write access to the data directory
2. **Database Locked**: Check if another process is accessing the database file
3. **Migration Failures**: Verify the database file is not corrupted

### Diagnostic Information

The health check endpoint (`/health`) provides detailed SQLite diagnostics:

```json
{
  "status": "Healthy",
  "results": {
    "sqlite_database": {
      "status": "Healthy",
      "data": {
        "database_path": "/app/data/quokkapack.db",
        "file_size": "2.1 MB",
        "database_size": "2.0 MB",
        "page_count": 512,
        "page_size": 4096,
        "journal_mode": "wal",
        "foreign_keys_enabled": true,
        "last_modified": "2024-01-01 12:00:00"
      }
    }
  }
}
```

### Backup and Recovery

Create database backups using the SQLite service:

```csharp
var sqliteService = serviceProvider.GetService<SQLiteDatabaseService>();
await sqliteService.BackupDatabaseAsync("/backup/path/backup.db");
```

## Performance Considerations

### Advantages of SQLite for Self-Hosting

- **Zero Configuration**: No separate database server required
- **Single File**: Easy backup and migration
- **Low Resource Usage**: Minimal memory and CPU overhead
- **ACID Compliance**: Full transaction support
- **Cross-Platform**: Works on all supported .NET platforms

### Limitations

- **Concurrent Writes**: Limited compared to SQL Server
- **Database Size**: Best for databases under 1TB
- **Network Access**: Local file access only
- **Advanced Features**: Some SQL Server features not available

### Recommendations

- Use SQLite for self-hosting and development
- Use SQL Server for production multi-user scenarios
- Monitor database size and performance regularly
- Implement regular backup procedures for important data

## Migration Between Database Providers

The application supports seamless migration between SQL Server and SQLite:

1. **Export Data**: Use Entity Framework migrations or data export tools
2. **Update Configuration**: Change connection string and provider settings
3. **Run Migrations**: The system will automatically apply schema changes
4. **Import Data**: Restore data using appropriate tools

The same Entity Framework models and migrations work with both providers, ensuring consistency across environments.
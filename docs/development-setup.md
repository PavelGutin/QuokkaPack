# Development Environment Setup

This document explains how to use the development Docker Compose configuration for QuokkaPack.

## Quick Start

1. **Start the development environment:**
   ```bash
   docker-compose -f docker-compose.dev.yml up --build
   ```

2. **Access the applications:**
   - API: http://localhost:7100
   - Razor Pages: http://localhost:7200
   - Blazor: http://localhost:7300
   - Angular: http://localhost:7400
   - SQL Server: localhost:1433

## Development Features

### Hot Reload Support
- **Source code volumes** are mounted for all .NET projects
- **Angular development server** runs with file watching enabled
- Changes to source code will automatically trigger rebuilds

### Debug Port Exposure
- API Debug Port: 5000
- Razor Debug Port: 5001
- Blazor Debug Port: 5002

### Development Database
- Uses SQL Server Developer edition
- Automatic database initialization
- Development data persisted in `./data/dev/sql`
- Relaxed health checks for faster startup

### Environment Configuration
- Uses `.env.dev` for development-specific variables
- Debug logging enabled by default
- Development-friendly connection strings

## Usage Commands

### Start Development Environment
```bash
# Start all services
docker-compose -f docker-compose.dev.yml up

# Start with rebuild
docker-compose -f docker-compose.dev.yml up --build

# Start in background
docker-compose -f docker-compose.dev.yml up -d
```

### Stop Development Environment
```bash
# Stop all services
docker-compose -f docker-compose.dev.yml down

# Stop and remove volumes
docker-compose -f docker-compose.dev.yml down -v
```

### View Logs
```bash
# View all logs
docker-compose -f docker-compose.dev.yml logs

# View specific service logs
docker-compose -f docker-compose.dev.yml logs quokkapack.api
docker-compose -f docker-compose.dev.yml logs sqlserver
```

### Database Operations
```bash
# Connect to SQL Server
docker-compose -f docker-compose.dev.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrongPassword123!

# Run Entity Framework migrations
docker-compose -f docker-compose.dev.yml exec quokkapack.api dotnet ef database update
```

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure ports 7100-7400, 1433, and 5000-5002 are available
2. **Volume permissions**: On Linux/Mac, ensure Docker has permission to mount source directories
3. **Database connection**: Wait for SQL Server health check to pass before accessing applications

### Health Checks
All services include health checks with relaxed timeouts for development:
- **Interval**: 60 seconds
- **Timeout**: 30 seconds
- **Start Period**: 120 seconds (180s for Angular)
- **Retries**: 5

### Performance Tips
- Use `--build` flag only when Dockerfiles change
- Consider using `docker-compose -f docker-compose.dev.yml up api razor` to start only needed services
- Monitor resource usage with `docker stats`

## File Structure
```
├── docker-compose.dev.yml          # Development compose configuration
├── .env.dev                        # Development environment variables
├── scripts/
│   └── init-dev-db.sql            # Database initialization script
├── data/
│   └── dev/
│       └── sql/                   # SQL Server development data
└── docs/
    └── development-setup.md       # This documentation
```
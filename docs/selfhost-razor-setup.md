# QuokkaPack Razor Self-Host Setup

This document describes how to deploy QuokkaPack with the Razor frontend as a single self-contained Docker container.

## Overview

The self-host Razor image (`Dockerfile.selfhost.razor`) combines:
- QuokkaPack API with SQLite database
- QuokkaPack Razor frontend
- nginx reverse proxy for routing
- Automatic database migration and seeding
- Data persistence through volume mounts

## Quick Start

### Build the Image

```bash
# Build the self-host image
docker build -f Dockerfile.selfhost.razor -t quokkapack-selfhost-razor .
```

### Run the Container

```bash
# Create data directory for persistence
mkdir -p ./quokkapack-data

# Run the container
docker run -d \
  --name quokkapack-selfhost-razor \
  -p 8080:80 \
  -v ./quokkapack-data:/app/data \
  -e JWT_SECRET="$(openssl rand -base64 32)" \
  quokkapack-selfhost-razor
```

### Access the Application

- **Web Interface**: http://localhost:8080
- **Health Check**: http://localhost:8080/health

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `JWT_SECRET` | JWT signing secret (auto-generated if not provided) | Auto-generated |
| `SELFHOST_DATA_PATH` | Path to data directory inside container | `/app/data` |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | `Production` |

### Volume Mounts

| Host Path | Container Path | Description |
|-----------|----------------|-------------|
| `./quokkapack-data` | `/app/data` | SQLite database and application data |

## Using PowerShell Script

A PowerShell script is provided for easier management:

```powershell
# Build and run
./scripts/build-selfhost-razor.ps1 -Run

# Build only
./scripts/build-selfhost-razor.ps1

# Run with custom data path
./scripts/build-selfhost-razor.ps1 -Run -DataPath "C:\MyData\QuokkaPack"
```

## Container Management

### View Logs
```bash
docker logs -f quokkapack-selfhost-razor
```

### Stop Container
```bash
docker stop quokkapack-selfhost-razor
```

### Remove Container
```bash
docker rm quokkapack-selfhost-razor
```

### Update Application
```bash
# Stop and remove old container
docker stop quokkapack-selfhost-razor
docker rm quokkapack-selfhost-razor

# Rebuild image
docker build -f Dockerfile.selfhost.razor -t quokkapack-selfhost-razor .

# Start new container (data persists in volume)
docker run -d \
  --name quokkapack-selfhost-razor \
  -p 8080:80 \
  -v ./quokkapack-data:/app/data \
  -e JWT_SECRET="your-existing-secret" \
  quokkapack-selfhost-razor
```

## Architecture

The self-host container runs multiple processes managed by supervisor:

1. **nginx** (port 80) - Reverse proxy and static file serving
   - Routes `/` to Razor frontend (port 5000)
   - Routes `/api/` to API backend (port 5001)
   - Serves health checks

2. **QuokkaPack.Razor** (port 5000) - Frontend application
   - Razor Pages UI
   - Authentication handling
   - API client integration

3. **QuokkaPack.API** (port 5001) - Backend API
   - REST API endpoints
   - SQLite database with Entity Framework
   - JWT authentication
   - Automatic database migration and seeding

## Database

- **Type**: SQLite
- **Location**: `/app/data/quokkapack.db`
- **Initialization**: Automatic on first startup
- **Migrations**: Applied automatically on startup
- **Seeding**: Default categories and items created automatically

## Security

- Container runs with non-root user where possible
- JWT secrets are auto-generated if not provided
- SQLite database is stored in persistent volume
- nginx handles SSL termination (if configured)

## Troubleshooting

### Container Won't Start
```bash
# Check container logs
docker logs quokkapack-selfhost-razor

# Check if port is already in use
netstat -an | grep :8080
```

### Database Issues
```bash
# Access container shell
docker exec -it quokkapack-selfhost-razor /bin/bash

# Check database file
ls -la /app/data/
sqlite3 /app/data/quokkapack.db ".tables"
```

### Application Not Accessible
```bash
# Check nginx status inside container
docker exec quokkapack-selfhost-razor supervisorctl status

# Check port mapping
docker port quokkapack-selfhost-razor
```

## Backup and Restore

### Backup Data
```bash
# Create backup of data directory
tar -czf quokkapack-backup-$(date +%Y%m%d).tar.gz ./quokkapack-data/
```

### Restore Data
```bash
# Stop container
docker stop quokkapack-selfhost-razor

# Restore data
tar -xzf quokkapack-backup-YYYYMMDD.tar.gz

# Start container
docker start quokkapack-selfhost-razor
```
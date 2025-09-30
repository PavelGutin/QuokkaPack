# QuokkaPack Angular Self-Host Setup

This document describes how to deploy QuokkaPack with the Angular frontend as a single self-contained Docker container.

## Overview

The self-host Angular image (`Dockerfile.selfhost.angular`) combines:
- QuokkaPack API with SQLite database
- QuokkaPack Angular SPA frontend
- nginx reverse proxy for routing and static file serving
- Automatic database migration and seeding
- Data persistence through volume mounts

## Quick Start

### Build the Image

```bash
# Build the self-host image
docker build -f Dockerfile.selfhost.angular -t quokkapack-selfhost-angular .
```

### Run the Container

```bash
# Create data directory for persistence
mkdir -p ./quokkapack-data

# Run the container
docker run -d \
  --name quokkapack-selfhost-angular \
  -p 8080:80 \
  -v ./quokkapack-data:/app/data \
  -e JWT_SECRET="$(openssl rand -base64 32)" \
  quokkapack-selfhost-angular
```

### Access the Application

- **Web Interface**: http://localhost:8080
- **API Documentation**: http://localhost:8080/swagger
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
./scripts/build-selfhost-angular.ps1 -Run

# Build only
./scripts/build-selfhost-angular.ps1

# Run with custom data path
./scripts/build-selfhost-angular.ps1 -Run -DataPath "C:\MyData\QuokkaPack"
```

## Container Management

### View Logs
```bash
docker logs -f quokkapack-selfhost-angular
```

### Stop Container
```bash
docker stop quokkapack-selfhost-angular
```

### Remove Container
```bash
docker rm quokkapack-selfhost-angular
```

### Update Application
```bash
# Stop and remove old container
docker stop quokkapack-selfhost-angular
docker rm quokkapack-selfhost-angular

# Rebuild image
docker build -f Dockerfile.selfhost.angular -t quokkapack-selfhost-angular .

# Start new container (data persists in volume)
docker run -d \
  --name quokkapack-selfhost-angular \
  -p 8080:80 \
  -v ./quokkapack-data:/app/data \
  -e JWT_SECRET="your-existing-secret" \
  quokkapack-selfhost-angular
```

## Architecture

The self-host container runs multiple processes managed by supervisor:

1. **nginx** (port 80) - Reverse proxy and Angular SPA serving
   - Serves Angular static files from `/usr/share/nginx/html`
   - Routes `/api/` to API backend (port 5001)
   - Routes `/swagger/` to API documentation
   - Handles Angular routing with fallback to `index.html`
   - Serves health checks

2. **QuokkaPack.API** (port 5001) - Backend API
   - REST API endpoints
   - SQLite database with Entity Framework
   - JWT authentication
   - Automatic database migration and seeding
   - Swagger/OpenAPI documentation

## Angular SPA Features

- **Single Page Application**: Client-side routing with Angular Router
- **API Integration**: TypeScript-generated API client from OpenAPI spec
- **Authentication**: JWT token-based authentication
- **Responsive Design**: Mobile-friendly UI
- **Progressive Web App**: Offline capabilities and app-like experience

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
- nginx security headers configured
- Content Security Policy (CSP) headers applied
- Angular build includes security optimizations

## Troubleshooting

### Container Won't Start
```bash
# Check container logs
docker logs quokkapack-selfhost-angular

# Check if port is already in use
netstat -an | grep :8080
```

### Database Issues
```bash
# Access container shell
docker exec -it quokkapack-selfhost-angular /bin/bash

# Check database file
ls -la /app/data/
sqlite3 /app/data/quokkapack.db ".tables"
```

### Application Not Accessible
```bash
# Check nginx and API status inside container
docker exec quokkapack-selfhost-angular supervisorctl status

# Check port mapping
docker port quokkapack-selfhost-angular

# Check nginx configuration
docker exec quokkapack-selfhost-angular nginx -t
```

### Angular Routing Issues
- Ensure nginx is configured with `try_files $uri $uri/ /index.html`
- Check browser console for JavaScript errors
- Verify API endpoints are accessible at `/api/`

## Backup and Restore

### Backup Data
```bash
# Create backup of data directory
tar -czf quokkapack-backup-$(date +%Y%m%d).tar.gz ./quokkapack-data/
```

### Restore Data
```bash
# Stop container
docker stop quokkapack-selfhost-angular

# Restore data
tar -xzf quokkapack-backup-YYYYMMDD.tar.gz

# Start container
docker start quokkapack-selfhost-angular
```

## Development Notes

### Angular Build Process
The container builds Angular in production mode with:
- Ahead-of-Time (AOT) compilation
- Tree shaking for smaller bundle size
- Minification and optimization
- Source map generation disabled for production

### API Integration
- TypeScript types are generated from the API's OpenAPI specification
- API client uses Angular's HttpClient with proper error handling
- JWT tokens are automatically included in API requests
- CORS is configured for cross-origin requests

### Performance Optimizations
- nginx gzip compression enabled
- Static asset caching with long expiration times
- Angular lazy loading for route-based code splitting
- Service worker for offline functionality (if enabled)
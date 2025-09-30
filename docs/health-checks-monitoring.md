# Health Checks and Monitoring

This document describes the comprehensive health checks and monitoring system implemented for QuokkaPack's containerized deployments.

## Overview

QuokkaPack implements a multi-layered health check and monitoring system that provides:

- **Service Health Monitoring**: Real-time health status of all services
- **Container-Friendly Logging**: Structured logging optimized for containerized environments
- **Metrics Collection**: Performance and health metrics for monitoring dashboards
- **Dependency Checking**: External service dependency validation
- **Detailed Diagnostics**: Rich health check responses with diagnostic information

## Health Check Endpoints

### Available Endpoints

All web services (API, Razor) expose the following health check endpoints:

| Endpoint | Purpose | Response Format |
|----------|---------|-----------------|
| `/health` | Basic health status | Simple text/JSON |
| `/health/ready` | Readiness check (all dependencies healthy) | Detailed JSON |
| `/health/live` | Liveness check (basic service availability) | Simple JSON |
| `/health/detailed` | Comprehensive diagnostics | Rich JSON with metrics |

### Frontend Services

- **Blazor WebAssembly**: `/health` endpoint served by nginx
- **Angular**: `/health` endpoint served by nginx with API proxy health

## Health Check Types

### 1. API Service Health Checks

**Location**: `QuokkaPack.ServerCommon.HealthChecks.ApiHealthCheck`

**Monitors**:
- Service availability
- Memory usage (warns at >500MB)
- Application version and environment
- Garbage collection statistics
- Process information

**Example Response**:
```json
{
  "status": "Healthy",
  "totalDuration": 45.2,
  "timestamp": "2024-01-15 10:30:00 UTC",
  "checks": [
    {
      "name": "api_service",
      "status": "Healthy",
      "duration": 12.5,
      "description": "API service is healthy",
      "data": {
        "service": "QuokkaPack.API",
        "version": "1.0.0",
        "environment": "Production",
        "memory_usage_mb": 245.67,
        "uptime_ms": 1234567
      }
    }
  ]
}
```

### 2. Database Health Checks

**SQL Server**: Built-in Entity Framework health check
**SQLite**: Custom `SQLiteDatabaseHealthCheck` with detailed diagnostics

**SQLite Diagnostics Include**:
- Database file size and path
- Page count and size
- Journal mode (recommends WAL)
- Foreign key constraint status
- Last modified timestamp

### 3. Web Application Health Checks

**Location**: `QuokkaPack.ServerCommon.HealthChecks.WebApplicationHealthCheck`

**Monitors**:
- Application availability
- Memory usage
- Thread count
- Container detection
- Process information

### 4. External Service Health Checks

**Location**: `QuokkaPack.ServerCommon.HealthChecks.ExternalServiceHealthCheck`

**Monitors**:
- API connectivity from frontend services
- Response time monitoring (warns at >5 seconds)
- HTTP status code validation
- Timeout detection

## Container Health Checks

### Docker Health Check Configuration

All Dockerfiles include health check instructions:

```dockerfile
# API/Razor Services
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health/ready || exit 1

# Nginx Services (Blazor/Angular)
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost/health || exit 1
```

### Docker Compose Health Checks

Development and production Docker Compose files include:

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health/ready"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 60s
```

## Logging Configuration

### Container-Friendly Logging

The logging system automatically detects containerized environments and adjusts output format:

**In Containers**:
- Structured JSON logging to stdout
- No file logging (relies on container log drivers)
- Enhanced with container metadata

**In Development**:
- Human-readable console output
- File logging to `Logs/` directory
- Detailed formatting for debugging

### Log Enrichment

All logs are enriched with:
- Application name and version
- Environment name
- Process ID and thread ID
- Container detection flag
- Machine name (when available)

### Health Check Logging

Health check events are logged at appropriate levels:
- **Information**: Successful health checks
- **Warning**: Degraded services
- **Error**: Failed health checks with exception details

## Metrics Collection

### Health Check Metrics

**Location**: `QuokkaPack.ServerCommon.Monitoring.HealthCheckMetrics`

**Collected Metrics**:
- `health_check_total`: Counter of health check executions
- `health_check_duration_seconds`: Histogram of health check durations
- `healthy_services_count`: Gauge of healthy service count

### Metrics Publishing

**Location**: `QuokkaPack.ServerCommon.Monitoring.HealthCheckPublisher`

**Features**:
- Automatic health check result publishing
- Structured logging of health events
- Metrics collection and aggregation
- Configurable publishing intervals

## Configuration

### Health Check Configuration

```csharp
// API Service
services.AddApiHealthChecks(configuration);

// Web Applications
services.AddWebApplicationHealthChecks(configuration, "QuokkaPack.Razor");

// Monitoring
services.AddContainerMonitoring();
services.AddHealthCheckLogging();
```

### Logging Configuration

```csharp
// Container-friendly logging
builder.Host.UseContainerFriendlyLogging("QuokkaPack.API");
```

### Docker Compose Environment Variables

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - Logging__LogLevel__Microsoft.Extensions.Diagnostics.HealthChecks=Information
  - Logging__LogLevel__QuokkaPack.ServerCommon.HealthChecks=Information
```

## Testing Health Checks

### Manual Testing

```bash
# Test basic health
curl http://localhost:7100/health

# Test detailed health with diagnostics
curl http://localhost:7100/health/detailed

# Test readiness (all dependencies)
curl http://localhost:7100/health/ready

# Test liveness (basic availability)
curl http://localhost:7100/health/live
```

### Automated Testing

Use the provided PowerShell script:

```powershell
# Test development environment
.\scripts\test-health-checks.ps1 -Environment dev

# Test with detailed output
.\scripts\test-health-checks.ps1 -Environment dev -Detailed
```

## Monitoring Integration

### Container Orchestration

Health checks integrate with:
- **Docker Compose**: Service dependency management
- **Docker Swarm**: Service health-based routing
- **Kubernetes**: Readiness and liveness probes (future)

### Log Aggregation

Structured logs are compatible with:
- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Fluentd** and **Fluent Bit**
- **Azure Monitor** and **Application Insights**
- **Prometheus** and **Grafana** (via metrics)

### Alerting

Health check failures can trigger alerts through:
- Log-based alerting systems
- Metrics-based monitoring (Prometheus AlertManager)
- Container orchestration health events

## Troubleshooting

### Common Issues

1. **Health Check Timeouts**
   - Increase timeout values in Docker health checks
   - Check service startup time and adjust `start_period`

2. **Database Connection Failures**
   - Verify connection strings
   - Check database container health
   - Review network connectivity between containers

3. **External Service Failures**
   - Verify API base URL configuration
   - Check network policies and firewall rules
   - Review service discovery configuration

### Debug Commands

```bash
# Check container health status
docker ps --format "table {{.Names}}\t{{.Status}}"

# View health check logs
docker logs <container_name> | grep -i health

# Test health endpoints directly
docker exec <container_name> curl -f http://localhost:8080/health/detailed
```

## Best Practices

1. **Health Check Design**
   - Keep health checks lightweight and fast
   - Include dependency checks in readiness probes
   - Use appropriate timeout and retry values

2. **Logging**
   - Use structured logging for better searchability
   - Include correlation IDs for request tracing
   - Log health check failures with sufficient context

3. **Monitoring**
   - Set up alerts for critical health check failures
   - Monitor health check response times
   - Track health check success rates over time

4. **Container Configuration**
   - Use different health check intervals for different environments
   - Configure appropriate resource limits
   - Implement graceful shutdown handling
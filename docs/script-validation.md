# QuokkaPack Container Management Script Validation

This document provides validation procedures and tests for all QuokkaPack container management scripts to ensure they work correctly.

## Script Validation Checklist

### Prerequisites Validation

Before running any validation tests, ensure:

```powershell
# Check PowerShell version (7.0+ required)
$PSVersionTable.PSVersion

# Check Docker installation
docker --version
docker-compose --version

# Check available disk space (10GB+ recommended)
Get-WmiObject -Class Win32_LogicalDisk | Where-Object {$_.DeviceID -eq "C:"} | Select-Object @{Name="FreeSpaceGB";Expression={[math]::Round($_.FreeSpace/1GB,2)}}

# Check available memory (8GB+ recommended)
Get-WmiObject -Class Win32_ComputerSystem | Select-Object @{Name="TotalMemoryGB";Expression={[math]::Round($_.TotalPhysicalMemory/1GB,2)}}
```

## Individual Script Validation

### 1. build-all-images.ps1

#### Basic Functionality Test
```powershell
# Test help display
.\scripts\build-all-images.ps1 -?

# Test parameter validation
.\scripts\build-all-images.ps1 -ImageType invalid  # Should fail with validation error

# Test dry run equivalent (build with verbose output)
.\scripts\build-all-images.ps1 -ImageType individual -Environment dev

# Verify images were created
docker images --filter "reference=quokkapack*:dev"
```

#### Advanced Tests
```powershell
# Test no-cache build
.\scripts\build-all-images.ps1 -ImageType individual -Environment dev -NoCache

# Test self-host images
.\scripts\build-all-images.ps1 -ImageType selfhost

# Test production builds
.\scripts\build-all-images.ps1 -Environment prod

# Test all images build
.\scripts\build-all-images.ps1
```

#### Expected Results
- All specified images should be built successfully
- Build times should be reasonable (< 10 minutes for all images)
- Images should have appropriate tags (dev/latest)
- No build errors in output

### 2. start-dev.ps1

#### Basic Functionality Test
```powershell
# Test help display
.\scripts\start-dev.ps1 -?

# Test basic start
.\scripts\start-dev.ps1 -Detached

# Verify services are running
docker-compose -f docker-compose.dev.yml ps

# Test service URLs
curl http://localhost:5000/health  # API
curl http://localhost:5001/health  # Razor
curl http://localhost:5002/health  # Blazor
curl http://localhost:4200/        # Angular
```

#### Advanced Tests
```powershell
# Test with build
.\scripts\start-dev.ps1 -Build -Detached

# Test specific services
.\scripts\start-dev.ps1 -Services "api,sqlserver" -Detached

# Test logs functionality
.\scripts\start-dev.ps1 -Logs

# Test foreground mode (Ctrl+C to stop)
.\scripts\start-dev.ps1
```

#### Expected Results
- All services should start successfully
- Health checks should pass within 2 minutes
- Service URLs should be accessible
- Database should be initialized with migrations

### 3. start-prod.ps1

#### Basic Functionality Test
```powershell
# Ensure production images exist
.\scripts\build-all-images.ps1 -Environment prod

# Test basic start
.\scripts\start-prod.ps1

# Verify services are running
docker-compose -f docker-compose.prod.yml ps

# Test HTTPS endpoints (may require certificates)
curl -k https://localhost:5443/health  # API
```

#### Advanced Tests
```powershell
# Test scaling
.\scripts\start-prod.ps1 -Scale "api=2,razor=2"

# Verify scaling worked
docker-compose -f docker-compose.prod.yml ps

# Test specific services
.\scripts\start-prod.ps1 -Services "api,sqlserver"
```

#### Expected Results
- Production services should start with HTTPS
- Health checks should be stricter than development
- Resource limits should be enforced
- Scaling should work correctly

### 4. stop-dev.ps1 and stop-prod.ps1

#### Basic Functionality Test
```powershell
# Start environment first
.\scripts\start-dev.ps1 -Detached

# Test basic stop
.\scripts\stop-dev.ps1

# Verify services are stopped
docker-compose -f docker-compose.dev.yml ps
```

#### Advanced Tests
```powershell
# Test with container removal
.\scripts\start-dev.ps1 -Detached
.\scripts\stop-dev.ps1 -Remove

# Test specific services stop
.\scripts\start-dev.ps1 -Services "api,razor" -Detached
.\scripts\stop-dev.ps1 -Services "api,razor"

# Test complete cleanup (WARNING: Deletes data)
.\scripts\start-dev.ps1 -Detached
.\scripts\stop-dev.ps1 -All
```

#### Expected Results
- Services should stop gracefully
- Containers should be removed when requested
- Data should be preserved unless explicitly removed
- No hanging processes

### 5. cleanup-containers.ps1

#### Basic Functionality Test
```powershell
# Test help display
.\scripts\cleanup-containers.ps1 -?

# Test dry run
.\scripts\cleanup-containers.ps1 -DryRun

# Test basic cleanup
.\scripts\cleanup-containers.ps1
```

#### Advanced Tests
```powershell
# Test image cleanup
.\scripts\cleanup-containers.ps1 -Type images -Environment dev

# Test system cleanup
.\scripts\cleanup-containers.ps1 -Type system -Prune

# Test complete cleanup with force
.\scripts\cleanup-containers.ps1 -Type all -Force
```

#### Expected Results
- Dry run should show what would be cleaned
- Cleanup should remove only intended resources
- System prune should free up disk space
- No active containers should be affected

### 6. maintenance.ps1

#### Basic Functionality Test
```powershell
# Test help display
.\scripts\maintenance.ps1 -?

# Test dry run
.\scripts\maintenance.ps1 -DryRun

# Test specific task
.\scripts\maintenance.ps1 -Task health -Environment dev
```

#### Advanced Tests
```powershell
# Start environment for testing
.\scripts\start-dev.ps1 -Detached

# Test backup task
.\scripts\maintenance.ps1 -Task backup

# Test log rotation
.\scripts\maintenance.ps1 -Task logs

# Test security scan
.\scripts\maintenance.ps1 -Task security

# Test performance collection
.\scripts\maintenance.ps1 -Task performance

# Test all tasks
.\scripts\maintenance.ps1 -Task all
```

#### Expected Results
- Maintenance directories should be created
- Backups should be created in maintenance/backups/
- Logs should be rotated and compressed
- Health reports should be generated
- Performance metrics should be collected

## Integration Testing

### End-to-End Development Workflow
```powershell
# 1. Clean start
.\scripts\cleanup-containers.ps1 -Type all -Force

# 2. Build images
.\scripts\build-all-images.ps1 -Environment dev

# 3. Start development environment
.\scripts\start-dev.ps1 -Build -Detached

# 4. Wait for services to be ready
Start-Sleep -Seconds 60

# 5. Test all endpoints
$endpoints = @(
    "http://localhost:5000/health",
    "http://localhost:5001/health", 
    "http://localhost:5002/health",
    "http://localhost:4200/"
)

foreach ($endpoint in $endpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint -TimeoutSec 10
        Write-Host "✓ $endpoint - Status: $($response.StatusCode)" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ $endpoint - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# 6. Run maintenance
.\scripts\maintenance.ps1 -Task health

# 7. Clean stop
.\scripts\stop-dev.ps1
```

### End-to-End Production Workflow
```powershell
# 1. Build production images
.\scripts\build-all-images.ps1 -Environment prod

# 2. Start production environment
.\scripts\start-prod.ps1

# 3. Wait for services to be ready
Start-Sleep -Seconds 90

# 4. Test HTTPS endpoints (skip certificate validation for testing)
$endpoints = @(
    "https://localhost:5443/health",
    "https://localhost:5444/health",
    "https://localhost:5445/health",
    "https://localhost:5446/"
)

foreach ($endpoint in $endpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint -SkipCertificateCheck -TimeoutSec 10
        Write-Host "✓ $endpoint - Status: $($response.StatusCode)" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ $endpoint - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# 5. Test scaling
.\scripts\start-prod.ps1 -Scale "api=2"

# 6. Verify scaling
$apiContainers = docker-compose -f docker-compose.prod.yml ps api
Write-Host "API containers running: $($apiContainers.Count)"

# 7. Run maintenance
.\scripts\maintenance.ps1 -Task all -Environment prod

# 8. Clean stop
.\scripts\stop-prod.ps1
```

### Self-Host Testing
```powershell
# 1. Build self-host images
.\scripts\build-all-images.ps1 -ImageType selfhost

# 2. Test Razor self-host
docker run -d --name test-razor-selfhost -p 8080:80 -v test-data:/app/data quokkapack-selfhost-razor:latest

# 3. Wait for startup
Start-Sleep -Seconds 30

# 4. Test endpoint
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8080/" -TimeoutSec 10
    Write-Host "✓ Razor self-host - Status: $($response.StatusCode)" -ForegroundColor Green
}
catch {
    Write-Host "✗ Razor self-host - Error: $($_.Exception.Message)" -ForegroundColor Red
}

# 5. Check data persistence
docker exec test-razor-selfhost ls -la /app/data

# 6. Clean up
docker stop test-razor-selfhost
docker rm test-razor-selfhost
docker volume rm test-data

# Repeat for Blazor and Angular self-host images
```

## Performance Validation

### Build Performance
```powershell
# Measure build times
$buildStart = Get-Date
.\scripts\build-all-images.ps1 -NoCache
$buildEnd = Get-Date
$buildDuration = $buildEnd - $buildStart

Write-Host "Total build time: $($buildDuration.TotalMinutes) minutes"

# Expected: < 10 minutes on modern hardware
if ($buildDuration.TotalMinutes -gt 10) {
    Write-Warning "Build time exceeds expected threshold"
}
```

### Startup Performance
```powershell
# Measure startup times
$startupStart = Get-Date
.\scripts\start-dev.ps1 -Detached

# Wait for all health checks to pass
do {
    Start-Sleep -Seconds 5
    $healthyServices = 0
    
    try { 
        Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 5 | Out-Null
        $healthyServices++
    } catch { }
    
    try { 
        Invoke-WebRequest -Uri "http://localhost:5001/health" -TimeoutSec 5 | Out-Null
        $healthyServices++
    } catch { }
    
    try { 
        Invoke-WebRequest -Uri "http://localhost:5002/health" -TimeoutSec 5 | Out-Null
        $healthyServices++
    } catch { }
    
    try { 
        Invoke-WebRequest -Uri "http://localhost:4200/" -TimeoutSec 5 | Out-Null
        $healthyServices++
    } catch { }
    
    $elapsed = (Get-Date) - $startupStart
    Write-Host "Elapsed: $($elapsed.TotalSeconds)s, Healthy services: $healthyServices/4"
    
} while ($healthyServices -lt 4 -and $elapsed.TotalMinutes -lt 5)

$startupEnd = Get-Date
$startupDuration = $startupEnd - $startupStart

Write-Host "Startup time: $($startupDuration.TotalSeconds) seconds"

# Expected: < 2 minutes
if ($startupDuration.TotalMinutes -gt 2) {
    Write-Warning "Startup time exceeds expected threshold"
}

.\scripts\stop-dev.ps1
```

### Resource Usage Validation
```powershell
# Start environment
.\scripts\start-dev.ps1 -Detached

# Wait for stabilization
Start-Sleep -Seconds 60

# Check resource usage
$stats = docker stats --no-stream --format json | ConvertFrom-Json

foreach ($stat in $stats) {
    $cpuPercent = [double]($stat.CPUPerc -replace '%', '')
    $memPercent = [double]($stat.MemPerc -replace '%', '')
    
    Write-Host "$($stat.Name): CPU $($stat.CPUPerc), Memory $($stat.MemPerc)"
    
    # Alert on high resource usage
    if ($cpuPercent -gt 50) {
        Write-Warning "$($stat.Name) has high CPU usage: $($stat.CPUPerc)"
    }
    
    if ($memPercent -gt 80) {
        Write-Warning "$($stat.Name) has high memory usage: $($stat.MemPerc)"
    }
}

.\scripts\stop-dev.ps1
```

## Error Handling Validation

### Network Port Conflicts
```powershell
# Simulate port conflict
Start-Process -FilePath "python" -ArgumentList "-m", "http.server", "5000" -NoNewWindow

# Try to start development environment (should handle gracefully)
try {
    .\scripts\start-dev.ps1 -Detached
    Write-Host "✗ Should have failed due to port conflict" -ForegroundColor Red
}
catch {
    Write-Host "✓ Correctly handled port conflict" -ForegroundColor Green
}

# Clean up
Get-Process | Where-Object {$_.ProcessName -eq "python"} | Stop-Process -Force
```

### Missing Dependencies
```powershell
# Test with Docker stopped
Stop-Service -Name "Docker Desktop Service" -Force -ErrorAction SilentlyContinue

try {
    .\scripts\build-all-images.ps1
    Write-Host "✗ Should have failed with Docker stopped" -ForegroundColor Red
}
catch {
    Write-Host "✓ Correctly detected Docker unavailable" -ForegroundColor Green
}

# Restart Docker
Start-Service -Name "Docker Desktop Service" -ErrorAction SilentlyContinue
```

### Disk Space Issues
```powershell
# Check current disk usage
$diskUsage = docker system df

# Simulate low disk space scenario by filling up with large images
# (This is a conceptual test - implement carefully in real scenarios)
Write-Host "Current Docker disk usage:"
Write-Host $diskUsage
```

## Automated Validation Script

Create a comprehensive validation script:

```powershell
# Save as scripts/validate-all.ps1
#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates all QuokkaPack container management scripts.

.DESCRIPTION
    Runs comprehensive validation tests on all container management scripts
    to ensure they work correctly.
#>

param(
    [switch]$Quick,
    [switch]$SkipCleanup
)

$ErrorActionPreference = 'Continue'
$testResults = @()

function Test-Script {
    param(
        [string]$Name,
        [scriptblock]$Test
    )
    
    Write-Host "Testing $Name..." -ForegroundColor Cyan
    
    try {
        $result = & $Test
        $testResults += @{ Name = $Name; Status = 'PASS'; Message = $result }
        Write-Host "✓ $Name - PASS" -ForegroundColor Green
    }
    catch {
        $testResults += @{ Name = $Name; Status = 'FAIL'; Message = $_.Exception.Message }
        Write-Host "✗ $Name - FAIL: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Run validation tests
Test-Script "Docker Availability" {
    docker --version | Out-Null
    "Docker is available"
}

Test-Script "Build All Images" {
    .\scripts\build-all-images.ps1 -ImageType individual -Environment dev
    "Images built successfully"
}

Test-Script "Start Development Environment" {
    .\scripts\start-dev.ps1 -Detached
    Start-Sleep -Seconds 60
    "Development environment started"
}

Test-Script "Health Check Endpoints" {
    $endpoints = @("http://localhost:5000/health", "http://localhost:5001/health")
    foreach ($endpoint in $endpoints) {
        Invoke-WebRequest -Uri $endpoint -TimeoutSec 10 | Out-Null
    }
    "All health checks passed"
}

Test-Script "Maintenance Tasks" {
    .\scripts\maintenance.ps1 -Task health -Environment dev
    "Maintenance tasks completed"
}

if (-not $SkipCleanup) {
    Test-Script "Stop and Cleanup" {
        .\scripts\stop-dev.ps1
        "Environment stopped successfully"
    }
}

# Generate report
Write-Host "`nValidation Results:" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow

$passCount = ($testResults | Where-Object { $_.Status -eq 'PASS' }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq 'FAIL' }).Count

foreach ($result in $testResults) {
    $color = if ($result.Status -eq 'PASS') { 'Green' } else { 'Red' }
    Write-Host "$($result.Status): $($result.Name)" -ForegroundColor $color
}

Write-Host "`nSummary: $passCount passed, $failCount failed" -ForegroundColor $(if ($failCount -eq 0) { 'Green' } else { 'Yellow' })

if ($failCount -gt 0) {
    exit 1
}
```

## Continuous Validation

### Scheduled Validation
```powershell
# Create a scheduled task for regular validation
$action = New-ScheduledTaskAction -Execute "pwsh.exe" -Argument "-File C:\path\to\QuokkaPack\scripts\validate-all.ps1 -Quick"
$trigger = New-ScheduledTaskTrigger -Daily -At "02:00AM"
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries

Register-ScheduledTask -TaskName "QuokkaPack Validation" -Action $action -Trigger $trigger -Settings $settings
```

### CI/CD Integration
```yaml
# GitHub Actions example
name: Container Script Validation
on:
  push:
    paths:
      - 'scripts/**'
      - 'docker-compose*.yml'
      - 'Dockerfile*'

jobs:
  validate:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - name: Validate Scripts
        run: .\scripts\validate-all.ps1
```

---

This validation guide ensures all container management scripts work correctly and provides a framework for ongoing testing and validation.
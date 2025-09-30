#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Tests health check endpoints for all QuokkaPack services
.DESCRIPTION
    This script tests the health check endpoints for API, Razor, Blazor, and Angular services
    to verify that comprehensive health checks and monitoring are working correctly.
.EXAMPLE
    .\test-health-checks.ps1
#>

param(
    [string]$Environment = "dev",
    [switch]$Detailed = $false
)

Write-Host "Testing QuokkaPack Health Checks" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Detailed: $Detailed" -ForegroundColor Yellow
Write-Host ""

# Define service endpoints based on environment
$services = @{
    "API" = @{
        "dev" = "http://localhost:7100"
        "prod" = "http://localhost:7100"
    }
    "Razor" = @{
        "dev" = "http://localhost:7200"
        "prod" = "http://localhost:7200"
    }
    "Blazor" = @{
        "dev" = "http://localhost:7300"
        "prod" = "http://localhost:7300"
    }
    "Angular" = @{
        "dev" = "http://localhost:7400"
        "prod" = "http://localhost:7400"
    }
}

# Health check endpoints to test
$healthEndpoints = @("/health", "/health/ready", "/health/live")
if ($Detailed) {
    $healthEndpoints += "/health/detailed"
}

$results = @()

foreach ($serviceName in $services.Keys) {
    $baseUrl = $services[$serviceName][$Environment]
    
    Write-Host "Testing $serviceName ($baseUrl)..." -ForegroundColor Cyan
    
    foreach ($endpoint in $healthEndpoints) {
        $url = "$baseUrl$endpoint"
        
        try {
            $response = Invoke-WebRequest -Uri $url -Method GET -TimeoutSec 10 -ErrorAction Stop
            
            $result = [PSCustomObject]@{
                Service = $serviceName
                Endpoint = $endpoint
                Status = "✅ Healthy"
                StatusCode = $response.StatusCode
                ResponseTime = "N/A"
                Details = if ($endpoint -eq "/health/detailed") { $response.Content } else { "OK" }
            }
            
            Write-Host "  $endpoint - " -NoNewline
            Write-Host "✅ $($response.StatusCode)" -ForegroundColor Green
            
            if ($Detailed -and $endpoint -eq "/health/detailed") {
                Write-Host "    Response: $($response.Content)" -ForegroundColor Gray
            }
        }
        catch {
            $statusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode } else { "Connection Failed" }
            
            $result = [PSCustomObject]@{
                Service = $serviceName
                Endpoint = $endpoint
                Status = "❌ Unhealthy"
                StatusCode = $statusCode
                ResponseTime = "N/A"
                Details = $_.Exception.Message
            }
            
            Write-Host "  $endpoint - " -NoNewline
            Write-Host "❌ $statusCode" -ForegroundColor Red
            
            if ($Detailed) {
                Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
        
        $results += $result
    }
    
    Write-Host ""
}

# Summary
Write-Host "Health Check Summary:" -ForegroundColor Yellow
Write-Host "===================" -ForegroundColor Yellow

$healthyCount = ($results | Where-Object { $_.Status -like "*Healthy*" }).Count
$totalCount = $results.Count

Write-Host "Total Checks: $totalCount" -ForegroundColor White
Write-Host "Healthy: " -NoNewline -ForegroundColor White
Write-Host "$healthyCount" -ForegroundColor Green
Write-Host "Unhealthy: " -NoNewline -ForegroundColor White
Write-Host "$($totalCount - $healthyCount)" -ForegroundColor Red

if ($healthyCount -eq $totalCount) {
    Write-Host "`n🎉 All health checks passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n⚠️  Some health checks failed. Check the services and try again." -ForegroundColor Yellow
    exit 1
}
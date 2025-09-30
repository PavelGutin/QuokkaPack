#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Tests SQLite database configuration for QuokkaPack self-hosting
.DESCRIPTION
    This script tests the SQLite database configuration by running the API with SQLite settings
    and verifying that the database is properly initialized and accessible.
.PARAMETER DataPath
    The path where SQLite database and logs should be stored (default: ./test-data)
.PARAMETER Port
    The port to run the test API on (default: 5000)
.PARAMETER Timeout
    Timeout in seconds for the test (default: 30)
#>

param(
    [string]$DataPath = "./test-data",
    [int]$Port = 5000,
    [int]$Timeout = 30
)

$ErrorActionPreference = "Stop"

Write-Host "Testing SQLite configuration for QuokkaPack..." -ForegroundColor Green

# Ensure data directory exists
if (!(Test-Path $DataPath)) {
    New-Item -ItemType Directory -Path $DataPath -Force | Out-Null
    Write-Host "Created test data directory: $DataPath" -ForegroundColor Yellow
}

# Set environment variables for SQLite self-host mode
$env:ASPNETCORE_ENVIRONMENT = "SelfHost"
$env:SELFHOST_DATA_PATH = (Resolve-Path $DataPath).Path
$env:ASPNETCORE_URLS = "http://localhost:$Port"

# Create temporary appsettings for testing
$testSettings = @{
    "Logging" = @{
        "LogLevel" = @{
            "Default" = "Information"
            "Microsoft.AspNetCore" = "Warning"
            "Microsoft.EntityFrameworkCore" = "Information"
        }
    }
    "ConnectionStrings" = @{
        "DefaultConnection" = "Data Source=$($env:SELFHOST_DATA_PATH)/test-quokkapack.db"
    }
    "SelfHost" = @{
        "DataPath" = $env:SELFHOST_DATA_PATH
        "EnableAutoSeeding" = $true
    }
    "JwtSettings" = @{
        "Secret" = "Test-Secret-Key-For-SQLite-Configuration-Testing-Only"
        "Issuer" = "QuokkaPack.Test"
        "Audience" = "QuokkaPack.Client"
    }
} | ConvertTo-Json -Depth 10

$testSettingsPath = "src/QuokkaPack.API/appsettings.SelfHost.json"
$originalSettings = $null

try {
    # Backup original settings if they exist
    if (Test-Path $testSettingsPath) {
        $originalSettings = Get-Content $testSettingsPath -Raw
    }
    
    # Write test settings
    $testSettings | Out-File -FilePath $testSettingsPath -Encoding UTF8
    Write-Host "Created test configuration file" -ForegroundColor Yellow

    # Build the project
    Write-Host "Building QuokkaPack.API..." -ForegroundColor Yellow
    dotnet build src/QuokkaPack.API/QuokkaPack.API.csproj --configuration Release --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }

    # Start the API in background
    Write-Host "Starting API with SQLite configuration..." -ForegroundColor Yellow
    $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "src/QuokkaPack.API", "--configuration", "Release", "--no-build" -PassThru -WindowStyle Hidden

    # Wait for API to start
    $startTime = Get-Date
    $apiStarted = $false
    
    while ((Get-Date) - $startTime -lt [TimeSpan]::FromSeconds($Timeout)) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:$Port/health" -Method GET -TimeoutSec 5 -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                $apiStarted = $true
                break
            }
        }
        catch {
            # API not ready yet, continue waiting
        }
        Start-Sleep -Seconds 1
    }

    if (-not $apiStarted) {
        throw "API failed to start within $Timeout seconds"
    }

    Write-Host "API started successfully!" -ForegroundColor Green

    # Test health endpoint
    Write-Host "Testing health endpoint..." -ForegroundColor Yellow
    $healthResponse = Invoke-WebRequest -Uri "http://localhost:$Port/health" -Method GET -UseBasicParsing
    $healthData = $healthResponse.Content | ConvertFrom-Json
    
    Write-Host "Health check status: $($healthData.status)" -ForegroundColor $(if ($healthData.status -eq "Healthy") { "Green" } else { "Yellow" })
    
    if ($healthData.results) {
        foreach ($check in $healthData.results.PSObject.Properties) {
            Write-Host "  - $($check.Name): $($check.Value.status)" -ForegroundColor $(if ($check.Value.status -eq "Healthy") { "Green" } else { "Yellow" })
        }
    }

    # Test database file creation
    $dbPath = Join-Path $DataPath "test-quokkapack.db"
    if (Test-Path $dbPath) {
        $dbInfo = Get-Item $dbPath
        Write-Host "Database file created successfully:" -ForegroundColor Green
        Write-Host "  - Path: $($dbInfo.FullName)" -ForegroundColor Gray
        Write-Host "  - Size: $([math]::Round($dbInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
        Write-Host "  - Created: $($dbInfo.CreationTime)" -ForegroundColor Gray
    } else {
        Write-Warning "Database file was not created at expected location: $dbPath"
    }

    # Test logs directory
    $logsPath = Join-Path $DataPath "logs"
    if (Test-Path $logsPath) {
        $logFiles = Get-ChildItem $logsPath -Filter "*.txt" | Sort-Object LastWriteTime -Descending
        if ($logFiles.Count -gt 0) {
            Write-Host "Log files created successfully:" -ForegroundColor Green
            $logFiles | Select-Object -First 3 | ForEach-Object {
                Write-Host "  - $($_.Name) ($([math]::Round($_.Length / 1KB, 2)) KB)" -ForegroundColor Gray
            }
        }
    }

    # Test API endpoints
    Write-Host "Testing API endpoints..." -ForegroundColor Yellow
    try {
        $swaggerResponse = Invoke-WebRequest -Uri "http://localhost:$Port/swagger/index.html" -Method GET -UseBasicParsing -TimeoutSec 5
        if ($swaggerResponse.StatusCode -eq 200) {
            Write-Host "  - Swagger UI: Available" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "  - Swagger UI: Not available" -ForegroundColor Yellow
    }

    Write-Host "`nSQLite configuration test completed successfully!" -ForegroundColor Green
    Write-Host "Database and logs are stored in: $DataPath" -ForegroundColor Gray

} catch {
    Write-Error "SQLite configuration test failed: $($_.Exception.Message)"
    exit 1
} finally {
    # Clean up
    if ($apiProcess -and !$apiProcess.HasExited) {
        Write-Host "Stopping API process..." -ForegroundColor Yellow
        $apiProcess.Kill()
        $apiProcess.WaitForExit(5000)
    }

    # Restore original settings
    if ($originalSettings) {
        $originalSettings | Out-File -FilePath $testSettingsPath -Encoding UTF8
        Write-Host "Restored original configuration file" -ForegroundColor Yellow
    } elseif (Test-Path $testSettingsPath) {
        Remove-Item $testSettingsPath -Force
        Write-Host "Removed test configuration file" -ForegroundColor Yellow
    }

    # Clean up environment variables
    Remove-Item Env:ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
    Remove-Item Env:SELFHOST_DATA_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:ASPNETCORE_URLS -ErrorAction SilentlyContinue
}

Write-Host "`nTest data remains in: $DataPath" -ForegroundColor Gray
Write-Host "You can clean it up manually if desired." -ForegroundColor Gray
# Generate OpenAPI spec by temporarily running the API
# Note: Skip build if called from MSBuild (already built)
if (-not $env:MSBUILD_RUNNING) {
    Write-Host "Building API project..." -ForegroundColor Cyan
    dotnet build $PSScriptRoot\..\src\QuokkaPack.API\QuokkaPack.API.csproj -c Release

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "Skipping build (already built by MSBuild)..." -ForegroundColor Cyan
}

Write-Host "Starting API to generate OpenAPI spec..." -ForegroundColor Cyan

# Create a new process start info with environment variables
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "dotnet"
$psi.Arguments = "run --project $PSScriptRoot\..\src\QuokkaPack.API --no-build -c Release"
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true
$psi.EnvironmentVariables["ASPNETCORE_URLS"] = "http://localhost:5000"
$psi.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development"

$apiProcess = [System.Diagnostics.Process]::Start($psi)

try {
    # Ensure artifacts directory exists
    $artifactsDir = "$PSScriptRoot\..\artifacts"
    if (-not (Test-Path $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir -Force | Out-Null
    }

    Write-Host "Fetching OpenAPI spec from http://localhost:5000/openapi/v1.json..." -ForegroundColor Cyan
    $maxRetries = 15
    $retryCount = 0
    $success = $false

    while ($retryCount -lt $maxRetries -and -not $success) {
        try {
            Start-Sleep -Seconds 2
            $response = Invoke-WebRequest -Uri "http://localhost:5000/openapi/v1.json" -TimeoutSec 5 -ErrorAction Stop

            if ($response.StatusCode -eq 200) {
                $response.Content | Out-File -FilePath "$artifactsDir\openapi.json" -Encoding UTF8
                $success = $true
                Write-Host "OpenAPI spec generated successfully!" -ForegroundColor Green
            }
        }
        catch {
            $retryCount++
            Write-Host "Waiting for API to start... (attempt $retryCount/$maxRetries)" -ForegroundColor Yellow
            if ($retryCount -eq 1) {
                Write-Host "Error details: $($_.Exception.Message)" -ForegroundColor DarkYellow
            }
        }
    }

    if (-not $success) {
        Write-Host "Failed to fetch OpenAPI spec after $maxRetries attempts" -ForegroundColor Red
        exit 1
    }

    # Generate TypeScript client using NSwag
    Write-Host "Generating TypeScript client..." -ForegroundColor Cyan
    Push-Location $PSScriptRoot\..\src\QuokkaPack.Angular
    try {
        dotnet nswag run codegen/nswag.json
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Angular API client generation completed successfully!" -ForegroundColor Green
            # Apply fixes to the generated client
            Write-Host "Applying fixes to generated client..." -ForegroundColor Cyan
            node codegen/fix-api-client.js
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    # Stop the API process and any children
    Write-Host "Stopping API..." -ForegroundColor Cyan
    if ($apiProcess -and !$apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    }
    # Also kill any dotnet processes on port 5000
    Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue | ForEach-Object {
        Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
    }
}

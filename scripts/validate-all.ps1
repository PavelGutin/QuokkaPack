#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates all QuokkaPack container management scripts.

.DESCRIPTION
    Runs comprehensive validation tests on all container management scripts
    to ensure they work correctly. This script performs end-to-end testing
    of the container management workflow.

.PARAMETER Quick
    Runs only essential validation tests (faster execution)

.PARAMETER SkipCleanup
    Leaves test environment running after validation

.PARAMETER Environment
    Target environment to validate. Valid values: 'dev', 'prod', 'both'
    Default: 'dev'

.PARAMETER Verbose
    Provides detailed output during validation

.EXAMPLE
    .\scripts\validate-all.ps1
    Runs full validation on development environment

.EXAMPLE
    .\scripts\validate-all.ps1 -Quick
    Runs essential validation tests only

.EXAMPLE
    .\scripts\validate-all.ps1 -Environment both -Verbose
    Validates both development and production environments with detailed output
#>

param(
    [switch]$Quick,
    [switch]$SkipCleanup,
    
    [ValidateSet('dev', 'prod', 'both')]
    [string]$Environment = 'dev',
    
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = 'Continue'

# Color functions for output
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }
function Write-Verbose { param($Message) if ($Verbose) { Write-Host $Message -ForegroundColor Gray } }

# Test results tracking
$script:testResults = @()
$script:startTime = Get-Date

function Add-TestResult {
    param(
        [string]$Name,
        [string]$Status,
        [string]$Message = "",
        [timespan]$Duration = [timespan]::Zero
    )
    
    $script:testResults += @{
        Name = $Name
        Status = $Status
        Message = $Message
        Duration = $Duration
        Timestamp = Get-Date
    }
}

function Test-Script {
    param(
        [string]$Name,
        [scriptblock]$Test,
        [switch]$Critical
    )
    
    Write-Info "Testing $Name..."
    $testStart = Get-Date
    
    try {
        $result = & $Test
        $testEnd = Get-Date
        $duration = $testEnd - $testStart
        
        Add-TestResult -Name $Name -Status 'PASS' -Message $result -Duration $duration
        Write-Success "✓ $Name - PASS ($($duration.TotalSeconds.ToString('F1'))s)"
        Write-Verbose "  Result: $result"
        return $true
    }
    catch {
        $testEnd = Get-Date
        $duration = $testEnd - $testStart
        
        Add-TestResult -Name $Name -Status 'FAIL' -Message $_.Exception.Message -Duration $duration
        Write-Error "✗ $Name - FAIL ($($duration.TotalSeconds.ToString('F1'))s)"
        Write-Error "  Error: $($_.Exception.Message)"
        
        if ($Critical) {
            Write-Error "Critical test failed. Stopping validation."
            throw "Critical validation failure: $Name"
        }
        
        return $false
    }
}

function Test-Prerequisites {
    Write-Info "Validating Prerequisites"
    Write-Info "========================"
    
    $allPassed = $true
    
    # Test PowerShell version
    $allPassed = (Test-Script "PowerShell Version" {
        $version = $PSVersionTable.PSVersion
        if ($version.Major -lt 7) {
            throw "PowerShell 7.0+ required. Current version: $version"
        }
        "PowerShell $version"
    } -Critical) -and $allPassed
    
    # Test Docker availability
    $allPassed = (Test-Script "Docker Availability" {
        docker --version | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Docker is not available or not running"
        }
        $version = docker --version
        "Docker is available: $version"
    } -Critical) -and $allPassed
    
    # Test Docker Compose
    $allPassed = (Test-Script "Docker Compose Availability" {
        docker-compose --version | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Docker Compose is not available"
        }
        $version = docker-compose --version
        "Docker Compose is available: $version"
    } -Critical) -and $allPassed
    
    # Test disk space
    $allPassed = (Test-Script "Disk Space Check" {
        $disk = Get-WmiObject -Class Win32_LogicalDisk | Where-Object {$_.DeviceID -eq "C:"}
        $freeSpaceGB = [math]::Round($disk.FreeSpace/1GB, 2)
        if ($freeSpaceGB -lt 5) {
            throw "Insufficient disk space. Available: ${freeSpaceGB}GB, Required: 5GB+"
        }
        "Available disk space: ${freeSpaceGB}GB"
    }) -and $allPassed
    
    # Test memory
    $allPassed = (Test-Script "Memory Check" {
        $memory = Get-WmiObject -Class Win32_ComputerSystem
        $totalMemoryGB = [math]::Round($memory.TotalPhysicalMemory/1GB, 2)
        if ($totalMemoryGB -lt 4) {
            Write-Warning "Low memory detected. Available: ${totalMemoryGB}GB, Recommended: 8GB+"
        }
        "Total memory: ${totalMemoryGB}GB"
    }) -and $allPassed
    
    return $allPassed
}

function Test-BuildScripts {
    Write-Info "`nValidating Build Scripts"
    Write-Info "========================"
    
    $allPassed = $true
    
    # Test build script help
    $allPassed = (Test-Script "Build Script Help" {
        $output = & .\scripts\build-all-images.ps1 -? 2>&1
        if ($output -match "SYNOPSIS") {
            "Help documentation is available"
        } else {
            throw "Help documentation not found or malformed"
        }
    }) -and $allPassed
    
    # Test parameter validation
    $allPassed = (Test-Script "Build Script Parameter Validation" {
        try {
            & .\scripts\build-all-images.ps1 -ImageType "invalid" 2>&1 | Out-Null
            throw "Should have failed with invalid parameter"
        }
        catch {
            if ($_.Exception.Message -match "Cannot validate argument") {
                "Parameter validation working correctly"
            } else {
                throw "Unexpected error: $($_.Exception.Message)"
            }
        }
    }) -and $allPassed
    
    # Test image building
    if ($Environment -eq 'dev' -or $Environment -eq 'both') {
        $allPassed = (Test-Script "Build Development Images" {
            & .\scripts\build-all-images.ps1 -ImageType individual -Environment dev
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to build development images"
            }
            
            # Verify images were created
            $images = docker images --filter "reference=quokkapack*:dev" --format "{{.Repository}}:{{.Tag}}"
            if ($images.Count -eq 0) {
                throw "No development images found after build"
            }
            
            "Built $($images.Count) development images: $($images -join ', ')"
        }) -and $allPassed
    }
    
    if (-not $Quick) {
        # Test self-host images
        $allPassed = (Test-Script "Build Self-Host Images" {
            & .\scripts\build-all-images.ps1 -ImageType selfhost
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to build self-host images"
            }
            
            $selfHostImages = docker images --filter "reference=quokkapack-selfhost*" --format "{{.Repository}}:{{.Tag}}"
            if ($selfHostImages.Count -eq 0) {
                throw "No self-host images found after build"
            }
            
            "Built $($selfHostImages.Count) self-host images: $($selfHostImages -join ', ')"
        }) -and $allPassed
    }
    
    return $allPassed
}

function Test-EnvironmentManagement {
    param([string]$EnvType)
    
    Write-Info "`nValidating $EnvType Environment Management"
    Write-Info "========================"
    
    $allPassed = $true
    $composeFile = if ($EnvType -eq 'dev') { 'docker-compose.dev.yml' } else { 'docker-compose.prod.yml' }
    $startScript = if ($EnvType -eq 'dev') { '.\scripts\start-dev.ps1' } else { '.\scripts\start-prod.ps1' }
    $stopScript = if ($EnvType -eq 'dev') { '.\scripts\stop-dev.ps1' } else { '.\scripts\stop-prod.ps1' }
    
    # Test start script help
    $allPassed = (Test-Script "$EnvType Start Script Help" {
        $output = & $startScript -? 2>&1
        if ($output -match "SYNOPSIS") {
            "Help documentation is available"
        } else {
            throw "Help documentation not found or malformed"
        }
    }) -and $allPassed
    
    # Test environment start
    $allPassed = (Test-Script "Start $EnvType Environment" {
        if ($EnvType -eq 'prod') {
            # Set auto-confirm for production
            $env:QUOKKAPACK_AUTO_CONFIRM = "true"
        }
        
        & $startScript -Detached
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to start $EnvType environment"
        }
        
        # Wait a moment for containers to initialize
        Start-Sleep -Seconds 10
        
        # Check container status
        $containers = & docker-compose -f $composeFile ps -q
        if (-not $containers) {
            throw "No containers found after starting $EnvType environment"
        }
        
        "Started $EnvType environment with $($containers.Count) containers"
    }) -and $allPassed
    
    if ($allPassed) {
        # Test health checks
        $allPassed = (Test-Script "$EnvType Health Checks" {
            $maxWaitTime = 120 # 2 minutes
            $waitTime = 0
            $healthyServices = 0
            $expectedServices = 4 # API, Razor, Blazor, Angular
            
            $basePort = if ($EnvType -eq 'dev') { 5000 } else { 5443 }
            $protocol = if ($EnvType -eq 'dev') { 'http' } else { 'https' }
            
            $endpoints = @(
                "$protocol://localhost:$basePort/health",
                "$protocol://localhost:$($basePort + 1)/health",
                "$protocol://localhost:$($basePort + 2)/health"
            )
            
            # Add Angular endpoint (different port structure)
            if ($EnvType -eq 'dev') {
                $endpoints += "http://localhost:4200/"
            } else {
                $endpoints += "https://localhost:5446/"
            }
            
            do {
                $healthyServices = 0
                
                foreach ($endpoint in $endpoints) {
                    try {
                        $params = @{
                            Uri = $endpoint
                            TimeoutSec = 5
                            UseBasicParsing = $true
                        }
                        
                        if ($protocol -eq 'https') {
                            $params.SkipCertificateCheck = $true
                        }
                        
                        Invoke-WebRequest @params | Out-Null
                        $healthyServices++
                        Write-Verbose "✓ $endpoint is healthy"
                    }
                    catch {
                        Write-Verbose "✗ $endpoint is not ready: $($_.Exception.Message)"
                    }
                }
                
                if ($healthyServices -lt $expectedServices) {
                    Start-Sleep -Seconds 5
                    $waitTime += 5
                    Write-Verbose "Waiting for services... ($healthyServices/$expectedServices healthy, ${waitTime}s elapsed)"
                }
                
            } while ($healthyServices -lt $expectedServices -and $waitTime -lt $maxWaitTime)
            
            if ($healthyServices -lt $expectedServices) {
                throw "Only $healthyServices/$expectedServices services are healthy after ${waitTime}s"
            }
            
            "All $expectedServices services are healthy (took ${waitTime}s)"
        }) -and $allPassed
        
        # Test specific service functionality
        if (-not $Quick) {
            $allPassed = (Test-Script "$EnvType Service Functionality" {
                $basePort = if ($EnvType -eq 'dev') { 5000 } else { 5443 }
                $protocol = if ($EnvType -eq 'dev') { 'http' } else { 'https' }
                
                # Test API Swagger endpoint
                $swaggerUrl = "$protocol://localhost:$basePort/swagger/index.html"
                $params = @{
                    Uri = $swaggerUrl
                    TimeoutSec = 10
                    UseBasicParsing = $true
                }
                
                if ($protocol -eq 'https') {
                    $params.SkipCertificateCheck = $true
                }
                
                $response = Invoke-WebRequest @params
                if ($response.StatusCode -ne 200) {
                    throw "Swagger UI not accessible"
                }
                
                "API Swagger UI is accessible"
            }) -and $allPassed
        }
    }
    
    # Test environment stop
    $allPassed = (Test-Script "Stop $EnvType Environment" {
        if ($EnvType -eq 'prod') {
            $env:QUOKKAPACK_AUTO_CONFIRM = "true"
        }
        
        & $stopScript
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to stop $EnvType environment"
        }
        
        # Verify containers are stopped
        Start-Sleep -Seconds 5
        $runningContainers = & docker-compose -f $composeFile ps -q --filter "status=running"
        if ($runningContainers) {
            throw "$($runningContainers.Count) containers still running after stop"
        }
        
        "Successfully stopped $EnvType environment"
    }) -and $allPassed
    
    return $allPassed
}

function Test-MaintenanceScripts {
    Write-Info "`nValidating Maintenance Scripts"
    Write-Info "============================="
    
    $allPassed = $true
    
    # Test maintenance script help
    $allPassed = (Test-Script "Maintenance Script Help" {
        $output = & .\scripts\maintenance.ps1 -? 2>&1
        if ($output -match "SYNOPSIS") {
            "Help documentation is available"
        } else {
            throw "Help documentation not found or malformed"
        }
    }) -and $allPassed
    
    # Test dry run functionality
    $allPassed = (Test-Script "Maintenance Dry Run" {
        & .\scripts\maintenance.ps1 -DryRun -Task all
        if ($LASTEXITCODE -ne 0) {
            throw "Maintenance dry run failed"
        }
        "Maintenance dry run completed successfully"
    }) -and $allPassed
    
    # Test cleanup script
    $allPassed = (Test-Script "Cleanup Script" {
        & .\scripts\cleanup-containers.ps1 -DryRun
        if ($LASTEXITCODE -ne 0) {
            throw "Cleanup script dry run failed"
        }
        "Cleanup script dry run completed successfully"
    }) -and $allPassed
    
    if (-not $Quick) {
        # Test actual maintenance tasks (requires running environment)
        $allPassed = (Test-Script "Maintenance Tasks with Environment" {
            # Start a minimal environment for testing
            & .\scripts\start-dev.ps1 -Services "api,sqlserver" -Detached
            Start-Sleep -Seconds 30
            
            try {
                # Test health check task
                & .\scripts\maintenance.ps1 -Task health -Environment dev
                if ($LASTEXITCODE -ne 0) {
                    throw "Health check task failed"
                }
                
                # Verify maintenance directories were created
                $maintenanceDirs = @("maintenance", "maintenance/backups", "maintenance/logs", "maintenance/reports")
                foreach ($dir in $maintenanceDirs) {
                    if (-not (Test-Path $dir)) {
                        throw "Maintenance directory not created: $dir"
                    }
                }
                
                "Maintenance tasks completed and directories created"
            }
            finally {
                # Clean up test environment
                & .\scripts\stop-dev.ps1 -Services "api,sqlserver"
            }
        }) -and $allPassed
    }
    
    return $allPassed
}

function Test-SelfHostDeployment {
    Write-Info "`nValidating Self-Host Deployment"
    Write-Info "==============================="
    
    $allPassed = $true
    
    if ($Quick) {
        Write-Warning "Skipping self-host deployment tests in quick mode"
        return $allPassed
    }
    
    # Test Razor self-host
    $allPassed = (Test-Script "Razor Self-Host Deployment" {
        $containerName = "test-razor-selfhost-$(Get-Random)"
        $volumeName = "test-data-$(Get-Random)"
        
        try {
            # Start self-host container
            docker run -d --name $containerName -p 8080:80 -v "${volumeName}:/app/data" quokkapack-selfhost-razor:latest
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to start Razor self-host container"
            }
            
            # Wait for startup
            Start-Sleep -Seconds 30
            
            # Test endpoint
            $maxRetries = 12 # 1 minute total
            $retries = 0
            $success = $false
            
            do {
                try {
                    $response = Invoke-WebRequest -Uri "http://localhost:8080/" -TimeoutSec 5 -UseBasicParsing
                    if ($response.StatusCode -eq 200) {
                        $success = $true
                        break
                    }
                }
                catch {
                    Start-Sleep -Seconds 5
                    $retries++
                }
            } while ($retries -lt $maxRetries)
            
            if (-not $success) {
                throw "Razor self-host endpoint not accessible after 1 minute"
            }
            
            # Check data persistence
            $dataFiles = docker exec $containerName ls -la /app/data 2>$null
            if (-not $dataFiles) {
                throw "Data directory not accessible in container"
            }
            
            "Razor self-host deployment successful"
        }
        finally {
            # Clean up
            docker stop $containerName 2>$null | Out-Null
            docker rm $containerName 2>$null | Out-Null
            docker volume rm $volumeName 2>$null | Out-Null
        }
    }) -and $allPassed
    
    return $allPassed
}

function Test-ContainerTests {
    Write-Info "`nValidating Container Tests"
    Write-Info "=========================="
    
    $allPassed = $true
    
    # Test container test project build
    $allPassed = (Test-Script "Container Test Project Build" {
        dotnet build tests/QuokkaPack.ContainerTests/QuokkaPack.ContainerTests.csproj --configuration Release
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build container test project"
        }
        "Container test project built successfully"
    }) -and $allPassed
    
    # Test build verification tests
    $allPassed = (Test-Script "Build Verification Tests" {
        dotnet test tests/QuokkaPack.ContainerTests/QuokkaPack.ContainerTests.csproj --configuration Release --filter "FullyQualifiedName~BuildVerificationTests" --logger console --verbosity quiet
        if ($LASTEXITCODE -ne 0) {
            throw "Build verification tests failed"
        }
        "Build verification tests passed"
    }) -and $allPassed
    
    # Test container test script
    $allPassed = (Test-Script "Container Test Script" {
        $output = & .\scripts\test-containers.ps1 -TestCategory BuildVerification 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Container test script failed: $output"
        }
        "Container test script executed successfully"
    }) -and $allPassed
    
    return $allPassed
}

function Test-ErrorHandling {
    Write-Info "`nValidating Error Handling"
    Write-Info "========================="
    
    $allPassed = $true
    
    # Test handling of missing Docker Compose file
    $allPassed = (Test-Script "Missing Compose File Handling" {
        $tempFile = "docker-compose.dev.yml.backup"
        
        try {
            # Backup original file
            if (Test-Path "docker-compose.dev.yml") {
                Move-Item "docker-compose.dev.yml" $tempFile
            }
            
            # Try to start environment (should fail gracefully)
            $output = & .\scripts\start-dev.ps1 2>&1
            $exitCode = $LASTEXITCODE
            
            if ($exitCode -eq 0) {
                throw "Should have failed with missing compose file"
            }
            
            if ($output -match "not found" -or $output -match "No such file") {
                "Correctly handled missing compose file"
            } else {
                throw "Did not properly report missing compose file"
            }
        }
        finally {
            # Restore original file
            if (Test-Path $tempFile) {
                Move-Item $tempFile "docker-compose.dev.yml"
            }
        }
    }) -and $allPassed
    
    # Test parameter validation
    $allPassed = (Test-Script "Script Parameter Validation" {
        $scripts = @(
            ".\scripts\build-all-images.ps1",
            ".\scripts\start-dev.ps1",
            ".\scripts\cleanup-containers.ps1",
            ".\scripts\maintenance.ps1"
        )
        
        $validationErrors = 0
        
        foreach ($script in $scripts) {
            try {
                # Test with invalid parameter (should fail)
                & $script -InvalidParameter "test" 2>$null
                $validationErrors++
            }
            catch {
                # Expected to fail - this is good
            }
        }
        
        if ($validationErrors -gt 0) {
            throw "$validationErrors scripts accepted invalid parameters"
        }
        
        "All scripts properly validate parameters"
    }) -and $allPassed
    
    return $allPassed
}

function Generate-Report {
    $endTime = Get-Date
    $totalDuration = $endTime - $script:startTime
    
    Write-Info "`nValidation Report"
    Write-Info "================="
    
    $passCount = ($script:testResults | Where-Object { $_.Status -eq 'PASS' }).Count
    $failCount = ($script:testResults | Where-Object { $_.Status -eq 'FAIL' }).Count
    $totalCount = $script:testResults.Count
    
    Write-Info "Total Tests: $totalCount"
    Write-Success "Passed: $passCount"
    if ($failCount -gt 0) {
        Write-Error "Failed: $failCount"
    } else {
        Write-Success "Failed: $failCount"
    }
    Write-Info "Total Duration: $($totalDuration.ToString('mm\:ss'))"
    
    Write-Info "`nDetailed Results:"
    Write-Info "-----------------"
    
    foreach ($result in $script:testResults) {
        $status = if ($result.Status -eq 'PASS') { '✓' } else { '✗' }
        $color = if ($result.Status -eq 'PASS') { 'Green' } else { 'Red' }
        $duration = $result.Duration.TotalSeconds.ToString('F1')
        
        Write-Host "$status $($result.Name) ($($duration)s)" -ForegroundColor $color
        
        if ($result.Status -eq 'FAIL' -and $result.Message) {
            Write-Host "  Error: $($result.Message)" -ForegroundColor Red
        } elseif ($Verbose -and $result.Message) {
            Write-Host "  Result: $($result.Message)" -ForegroundColor Gray
        }
    }
    
    # Generate summary file
    $reportFile = "validation-report-$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss').json"
    $reportData = @{
        Timestamp = $script:startTime
        Duration = $totalDuration.TotalSeconds
        Environment = $Environment
        Quick = $Quick.IsPresent
        Summary = @{
            Total = $totalCount
            Passed = $passCount
            Failed = $failCount
            SuccessRate = if ($totalCount -gt 0) { [math]::Round(($passCount / $totalCount) * 100, 2) } else { 0 }
        }
        Results = $script:testResults
    }
    
    $reportData | ConvertTo-Json -Depth 4 | Out-File -FilePath $reportFile -Encoding UTF8
    Write-Info "`nDetailed report saved to: $reportFile"
    
    return $failCount -eq 0
}

# Main execution
try {
    Write-Info "QuokkaPack Container Management Validation"
    Write-Info "=========================================="
    Write-Info "Environment: $Environment"
    Write-Info "Quick Mode: $($Quick.IsPresent)"
    Write-Info "Skip Cleanup: $($SkipCleanup.IsPresent)"
    Write-Info "Start Time: $($script:startTime.ToString('yyyy-MM-dd HH:mm:ss'))"
    Write-Info ""
    
    $overallSuccess = $true
    
    # Run validation phases
    $overallSuccess = (Test-Prerequisites) -and $overallSuccess
    $overallSuccess = (Test-BuildScripts) -and $overallSuccess
    
    if ($Environment -eq 'dev' -or $Environment -eq 'both') {
        $overallSuccess = (Test-EnvironmentManagement -EnvType 'dev') -and $overallSuccess
    }
    
    if ($Environment -eq 'prod' -or $Environment -eq 'both') {
        $overallSuccess = (Test-EnvironmentManagement -EnvType 'prod') -and $overallSuccess
    }
    
    $overallSuccess = (Test-MaintenanceScripts) -and $overallSuccess
    
    if (-not $Quick) {
        $overallSuccess = (Test-SelfHostDeployment) -and $overallSuccess
        $overallSuccess = (Test-ErrorHandling) -and $overallSuccess
        $overallSuccess = (Test-ContainerTests) -and $overallSuccess
    }
    
    # Generate final report
    $reportSuccess = Generate-Report
    $overallSuccess = $overallSuccess -and $reportSuccess
    
    if ($overallSuccess) {
        Write-Success "`n🎉 All validations passed successfully!"
        exit 0
    } else {
        Write-Error "`n❌ Some validations failed. Check the report for details."
        exit 1
    }
}
catch {
    Write-Error "Validation failed with critical error: $($_.Exception.Message)"
    Write-Error $_.ScriptStackTrace
    exit 1
}
finally {
    # Cleanup environment variables
    Remove-Item Env:QUOKKAPACK_AUTO_CONFIRM -ErrorAction SilentlyContinue
    
    # Final cleanup if not skipped
    if (-not $SkipCleanup) {
        Write-Info "`nPerforming final cleanup..."
        try {
            & .\scripts\stop-dev.ps1 2>$null | Out-Null
            & .\scripts\stop-prod.ps1 2>$null | Out-Null
        }
        catch {
            # Ignore cleanup errors
        }
    }
}
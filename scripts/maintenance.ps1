#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Performs regular maintenance tasks for QuokkaPack Docker containers.

.DESCRIPTION
    This script automates common maintenance tasks including:
    - Database backups and cleanup
    - Log rotation and archival
    - Image cleanup and optimization
    - Health monitoring and reporting
    - Performance metrics collection
    - Security updates and scanning

.PARAMETER Task
    Specific maintenance task to perform. Valid values:
    'backup', 'cleanup', 'logs', 'health', 'security', 'performance', 'all'
    Default: 'all'

.PARAMETER Environment
    Target environment. Valid values: 'dev', 'prod', 'both'
    Default: 'both'

.PARAMETER Schedule
    Run in scheduled mode (less verbose output)

.PARAMETER DryRun
    Show what would be done without actually performing tasks

.PARAMETER Force
    Skip confirmation prompts

.EXAMPLE
    .\maintenance.ps1
    Performs all maintenance tasks for both environments

.EXAMPLE
    .\maintenance.ps1 -Task backup -Environment prod
    Performs database backup for production only

.EXAMPLE
    .\maintenance.ps1 -Task cleanup -DryRun
    Shows what cleanup would be performed

.EXAMPLE
    .\maintenance.ps1 -Schedule
    Runs in scheduled mode with minimal output
#>

param(
    [ValidateSet('backup', 'cleanup', 'logs', 'health', 'security', 'performance', 'all')]
    [string]$Task = 'all',
    
    [ValidateSet('dev', 'prod', 'both')]
    [string]$Environment = 'both',
    
    [switch]$Schedule,
    [switch]$DryRun,
    [switch]$Force
)

# Set error action preference
$ErrorActionPreference = 'Stop'

# Color functions for output
function Write-Success { param($Message) if (-not $Schedule) { Write-Host $Message -ForegroundColor Green } }
function Write-Info { param($Message) if (-not $Schedule) { Write-Host $Message -ForegroundColor Cyan } }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

# Configuration
$MaintenanceDir = "maintenance"
$BackupDir = "$MaintenanceDir/backups"
$LogsDir = "$MaintenanceDir/logs"
$ReportsDir = "$MaintenanceDir/reports"

# Ensure maintenance directories exist
@($MaintenanceDir, $BackupDir, $LogsDir, $ReportsDir) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
    }
}

function Get-Timestamp {
    return Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
}

function Get-TargetEnvironments {
    param([string]$Env)
    
    switch ($Env) {
        'dev' { return @(@{ Name = 'dev'; ComposeFile = 'docker-compose.dev.yml'; Project = 'quokkapack-dev' }) }
        'prod' { return @(@{ Name = 'prod'; ComposeFile = 'docker-compose.prod.yml'; Project = 'quokkapack-prod' }) }
        'both' { 
            return @(
                @{ Name = 'dev'; ComposeFile = 'docker-compose.dev.yml'; Project = 'quokkapack-dev' },
                @{ Name = 'prod'; ComposeFile = 'docker-compose.prod.yml'; Project = 'quokkapack-prod' }
            )
        }
        default { return @() }
    }
}

function Backup-Database {
    param(
        [hashtable]$EnvConfig,
        [bool]$IsDryRun
    )
    
    Write-Info "Performing database backup for $($EnvConfig.Name) environment..."
    
    $timestamp = Get-Timestamp
    $backupFile = "$BackupDir/quokkapack_$($EnvConfig.Name)_$timestamp.bak"
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would create backup: $backupFile"
        return
    }
    
    try {
        # Check if SQL Server container is running
        $sqlContainer = & docker-compose -f $EnvConfig.ComposeFile -p $EnvConfig.Project ps -q sqlserver 2>$null
        if (-not $sqlContainer) {
            Write-Warning "SQL Server container not running for $($EnvConfig.Name) environment. Skipping backup."
            return
        }
        
        # Create backup
        $backupCmd = "BACKUP DATABASE QuokkaPack TO DISK = '/var/opt/mssql/backup/backup_$timestamp.bak' WITH FORMAT, INIT"
        & docker-compose -f $EnvConfig.ComposeFile -p $EnvConfig.Project exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P `$SA_PASSWORD -Q "$backupCmd"
        
        if ($LASTEXITCODE -eq 0) {
            # Copy backup out of container
            & docker cp "$($sqlContainer):/var/opt/mssql/backup/backup_$timestamp.bak" $backupFile
            
            if (Test-Path $backupFile) {
                $backupSize = (Get-Item $backupFile).Length / 1MB
                Write-Success "✓ Database backup created: $backupFile ($([math]::Round($backupSize, 2)) MB)"
                
                # Clean up old backups (keep last 7 days)
                $cutoffDate = (Get-Date).AddDays(-7)
                Get-ChildItem $BackupDir -Filter "quokkapack_$($EnvConfig.Name)_*.bak" | 
                    Where-Object { $_.CreationTime -lt $cutoffDate } | 
                    ForEach-Object {
                        Remove-Item $_.FullName -Force
                        Write-Info "Removed old backup: $($_.Name)"
                    }
            } else {
                throw "Backup file was not created successfully"
            }
        } else {
            throw "Database backup command failed"
        }
    }
    catch {
        Write-Error "Database backup failed for $($EnvConfig.Name): $($_.Exception.Message)"
    }
}

function Cleanup-Resources {
    param(
        [hashtable]$EnvConfig,
        [bool]$IsDryRun
    )
    
    Write-Info "Cleaning up resources for $($EnvConfig.Name) environment..."
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would clean up stopped containers and unused images"
        return
    }
    
    try {
        # Remove stopped containers
        $stoppedContainers = & docker-compose -f $EnvConfig.ComposeFile -p $EnvConfig.Project ps -q --filter "status=exited" 2>$null
        if ($stoppedContainers) {
            & docker rm $stoppedContainers 2>$null
            Write-Success "✓ Removed stopped containers"
        }
        
        # Clean up unused images (keep last 2 versions)
        $images = @('quokkapack-api', 'quokkapack-razor', 'quokkapack-blazor', 'quokkapack-angular')
        foreach ($image in $images) {
            $tag = if ($EnvConfig.Name -eq 'prod') { 'latest' } else { 'dev' }
            $oldImages = & docker images --filter "reference=$image" --filter "dangling=false" --format "{{.ID}}" | Select-Object -Skip 2
            
            if ($oldImages) {
                foreach ($imageId in $oldImages) {
                    try {
                        & docker rmi $imageId -f 2>$null
                    }
                    catch {
                        # Image might be in use, continue
                    }
                }
            }
        }
        
        Write-Success "✓ Cleaned up old images"
    }
    catch {
        Write-Warning "Some cleanup operations failed: $($_.Exception.Message)"
    }
}

function Rotate-Logs {
    param(
        [hashtable]$EnvConfig,
        [bool]$IsDryRun
    )
    
    Write-Info "Rotating logs for $($EnvConfig.Name) environment..."
    
    $timestamp = Get-Timestamp
    $logFile = "$LogsDir/quokkapack_$($EnvConfig.Name)_$timestamp.log"
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would rotate logs to: $logFile"
        return
    }
    
    try {
        # Export current logs
        & docker-compose -f $EnvConfig.ComposeFile -p $EnvConfig.Project logs --since=24h > $logFile
        
        if (Test-Path $logFile) {
            $logSize = (Get-Item $logFile).Length / 1MB
            Write-Success "✓ Logs exported: $logFile ($([math]::Round($logSize, 2)) MB)"
            
            # Compress old logs (older than 1 day)
            $cutoffDate = (Get-Date).AddDays(-1)
            Get-ChildItem $LogsDir -Filter "quokkapack_$($EnvConfig.Name)_*.log" | 
                Where-Object { $_.CreationTime -lt $cutoffDate -and $_.Extension -eq '.log' } | 
                ForEach-Object {
                    $compressedFile = $_.FullName -replace '\.log$', '.zip'
                    Compress-Archive -Path $_.FullName -DestinationPath $compressedFile -Force
                    Remove-Item $_.FullName -Force
                    Write-Info "Compressed log: $($_.Name)"
                }
            
            # Remove old compressed logs (keep last 30 days)
            $oldCutoffDate = (Get-Date).AddDays(-30)
            Get-ChildItem $LogsDir -Filter "quokkapack_$($EnvConfig.Name)_*.zip" | 
                Where-Object { $_.CreationTime -lt $oldCutoffDate } | 
                ForEach-Object {
                    Remove-Item $_.FullName -Force
                    Write-Info "Removed old log archive: $($_.Name)"
                }
        }
    }
    catch {
        Write-Error "Log rotation failed for $($EnvConfig.Name): $($_.Exception.Message)"
    }
}

function Check-Health {
    param(
        [hashtable]$EnvConfig,
        [bool]$IsDryRun
    )
    
    Write-Info "Checking health for $($EnvConfig.Name) environment..."
    
    $timestamp = Get-Timestamp
    $healthReport = "$ReportsDir/health_$($EnvConfig.Name)_$timestamp.json"
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would generate health report: $healthReport"
        return
    }
    
    try {
        $healthData = @{
            Timestamp = $timestamp
            Environment = $EnvConfig.Name
            Services = @{}
            System = @{}
        }
        
        # Check container status
        $containers = & docker-compose -f $EnvConfig.ComposeFile -p $EnvConfig.Project ps --format json 2>$null | ConvertFrom-Json
        
        foreach ($container in $containers) {
            $serviceName = $container.Service
            $healthStatus = & docker inspect --format='{{.State.Health.Status}}' $container.Name 2>$null
            
            $healthData.Services[$serviceName] = @{
                Status = $container.State
                Health = if ($healthStatus) { $healthStatus } else { 'unknown' }
                Uptime = $container.RunningFor
            }
        }
        
        # Check system resources
        $systemStats = & docker system df --format json 2>$null | ConvertFrom-Json
        $healthData.System = @{
            Images = $systemStats.Images
            Containers = $systemStats.Containers
            Volumes = $systemStats.LocalVolumes
            BuildCache = $systemStats.BuildCache
        }
        
        # Save health report
        $healthData | ConvertTo-Json -Depth 3 | Out-File -FilePath $healthReport -Encoding UTF8
        
        # Check for issues
        $issues = @()
        foreach ($service in $healthData.Services.Keys) {
            $serviceData = $healthData.Services[$service]
            if ($serviceData.Status -ne 'running') {
                $issues += "Service $service is not running (Status: $($serviceData.Status))"
            }
            if ($serviceData.Health -eq 'unhealthy') {
                $issues += "Service $service is unhealthy"
            }
        }
        
        if ($issues.Count -eq 0) {
            Write-Success "✓ All services healthy in $($EnvConfig.Name) environment"
        } else {
            Write-Warning "Health issues found in $($EnvConfig.Name) environment:"
            foreach ($issue in $issues) {
                Write-Warning "  • $issue"
            }
        }
        
        Write-Info "Health report saved: $healthReport"
    }
    catch {
        Write-Error "Health check failed for $($EnvConfig.Name): $($_.Exception.Message)"
    }
}

function Scan-Security {
    param(
        [hashtable]$EnvConfig,
        [bool]$IsDryRun
    )
    
    Write-Info "Performing security scan for $($EnvConfig.Name) environment..."
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would perform security scan"
        return
    }
    
    try {
        $images = @('quokkapack-api', 'quokkapack-razor', 'quokkapack-blazor', 'quokkapack-angular')
        $tag = if ($EnvConfig.Name -eq 'prod') { 'latest' } else { 'dev' }
        
        $vulnerabilities = @()
        
        foreach ($image in $images) {
            $imageName = "${image}:${tag}"
            
            # Check if image exists
            $imageExists = & docker images --filter "reference=$imageName" --format "{{.Repository}}" 2>$null
            if (-not $imageExists) {
                continue
            }
            
            Write-Info "Scanning $imageName..."
            
            # Use Docker Scout if available, otherwise skip
            try {
                $scanResult = & docker scout cves $imageName --format json 2>$null
                if ($LASTEXITCODE -eq 0 -and $scanResult) {
                    $scanData = $scanResult | ConvertFrom-Json
                    if ($scanData.vulnerabilities) {
                        $highVulns = $scanData.vulnerabilities | Where-Object { $_.severity -eq 'high' -or $_.severity -eq 'critical' }
                        if ($highVulns) {
                            $vulnerabilities += @{
                                Image = $imageName
                                HighSeverityCount = $highVulns.Count
                                Vulnerabilities = $highVulns
                            }
                        }
                    }
                }
            }
            catch {
                Write-Warning "Security scan not available for $imageName (Docker Scout not installed)"
            }
        }
        
        if ($vulnerabilities.Count -eq 0) {
            Write-Success "✓ No high-severity vulnerabilities found"
        } else {
            Write-Warning "Security vulnerabilities found:"
            foreach ($vuln in $vulnerabilities) {
                Write-Warning "  • $($vuln.Image): $($vuln.HighSeverityCount) high-severity vulnerabilities"
            }
            
            # Save detailed report
            $timestamp = Get-Timestamp
            $securityReport = "$ReportsDir/security_$($EnvConfig.Name)_$timestamp.json"
            $vulnerabilities | ConvertTo-Json -Depth 5 | Out-File -FilePath $securityReport -Encoding UTF8
            Write-Info "Detailed security report saved: $securityReport"
        }
    }
    catch {
        Write-Error "Security scan failed for $($EnvConfig.Name): $($_.Exception.Message)"
    }
}

function Collect-Performance {
    param(
        [hashtable]$EnvConfig,
        [bool]$IsDryRun
    )
    
    Write-Info "Collecting performance metrics for $($EnvConfig.Name) environment..."
    
    $timestamp = Get-Timestamp
    $perfReport = "$ReportsDir/performance_$($EnvConfig.Name)_$timestamp.json"
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would collect performance metrics: $perfReport"
        return
    }
    
    try {
        # Get container stats
        $stats = & docker stats --no-stream --format json 2>$null
        if ($stats) {
            $statsData = $stats | ConvertFrom-Json
            
            $perfData = @{
                Timestamp = $timestamp
                Environment = $EnvConfig.Name
                ContainerStats = $statsData
                SystemInfo = @{}
            }
            
            # Add system information
            $systemInfo = & docker system info --format json 2>$null | ConvertFrom-Json
            $perfData.SystemInfo = @{
                ContainersRunning = $systemInfo.ContainersRunning
                ContainersStopped = $systemInfo.ContainersStopped
                Images = $systemInfo.Images
                MemTotal = $systemInfo.MemTotal
                NCPU = $systemInfo.NCPU
            }
            
            # Save performance report
            $perfData | ConvertTo-Json -Depth 3 | Out-File -FilePath $perfReport -Encoding UTF8
            
            # Analyze performance
            $highCpuContainers = $statsData | Where-Object { 
                $_.CPUPerc -replace '%', '' -as [double] -gt 80 
            }
            
            $highMemContainers = $statsData | Where-Object { 
                $_.MemPerc -replace '%', '' -as [double] -gt 80 
            }
            
            if ($highCpuContainers -or $highMemContainers) {
                Write-Warning "Performance issues detected:"
                foreach ($container in $highCpuContainers) {
                    Write-Warning "  • High CPU: $($container.Name) ($($container.CPUPerc))"
                }
                foreach ($container in $highMemContainers) {
                    Write-Warning "  • High Memory: $($container.Name) ($($container.MemPerc))"
                }
            } else {
                Write-Success "✓ Performance metrics within normal ranges"
            }
            
            Write-Info "Performance report saved: $perfReport"
        }
    }
    catch {
        Write-Error "Performance collection failed for $($EnvConfig.Name): $($_.Exception.Message)"
    }
}

function Generate-Summary {
    param([array]$Environments)
    
    if ($Schedule) {
        return  # Skip summary in scheduled mode
    }
    
    Write-Info ""
    Write-Info "Maintenance Summary"
    Write-Info "=================="
    
    foreach ($env in $Environments) {
        Write-Info "Environment: $($env.Name)"
        
        # Count recent files
        $recentBackups = Get-ChildItem $BackupDir -Filter "quokkapack_$($env.Name)_*.bak" -ErrorAction SilentlyContinue | 
            Where-Object { $_.CreationTime -gt (Get-Date).AddHours(-1) }
        
        $recentLogs = Get-ChildItem $LogsDir -Filter "quokkapack_$($env.Name)_*.log" -ErrorAction SilentlyContinue | 
            Where-Object { $_.CreationTime -gt (Get-Date).AddHours(-1) }
        
        $recentReports = Get-ChildItem $ReportsDir -Filter "*_$($env.Name)_*.json" -ErrorAction SilentlyContinue | 
            Where-Object { $_.CreationTime -gt (Get-Date).AddHours(-1) }
        
        Write-Info "  • Backups created: $($recentBackups.Count)"
        Write-Info "  • Logs rotated: $($recentLogs.Count)"
        Write-Info "  • Reports generated: $($recentReports.Count)"
    }
    
    Write-Info ""
    Write-Info "Maintenance files location: $MaintenanceDir"
    Write-Info "Next scheduled maintenance: $(if ($Schedule) { 'Running now' } else { 'Manual execution' })"
}

# Main execution
try {
    if (-not $Schedule) {
        Write-Info "QuokkaPack Container Maintenance"
        Write-Info "================================"
        Write-Info "Task: $Task"
        Write-Info "Environment: $Environment"
        Write-Info "Dry Run: $DryRun"
        Write-Info ""
    }
    
    # Verify Docker is available
    try {
        docker --version | Out-Null
    }
    catch {
        throw "Docker is not available. Please ensure Docker is installed and running."
    }
    
    $environments = Get-TargetEnvironments -Env $Environment
    $startTime = Get-Date
    
    foreach ($env in $environments) {
        if (-not $Schedule) {
            Write-Info "Processing $($env.Name) environment..."
        }
        
        # Check if environment is running (for some tasks)
        $runningContainers = & docker-compose -f $env.ComposeFile -p $env.Project ps -q 2>$null
        
        switch ($Task) {
            'backup' {
                if ($runningContainers) {
                    Backup-Database -EnvConfig $env -IsDryRun $DryRun
                } else {
                    Write-Warning "Environment $($env.Name) is not running. Skipping backup."
                }
            }
            'cleanup' {
                Cleanup-Resources -EnvConfig $env -IsDryRun $DryRun
            }
            'logs' {
                if ($runningContainers) {
                    Rotate-Logs -EnvConfig $env -IsDryRun $DryRun
                } else {
                    Write-Warning "Environment $($env.Name) is not running. Skipping log rotation."
                }
            }
            'health' {
                if ($runningContainers) {
                    Check-Health -EnvConfig $env -IsDryRun $DryRun
                } else {
                    Write-Warning "Environment $($env.Name) is not running. Skipping health check."
                }
            }
            'security' {
                Scan-Security -EnvConfig $env -IsDryRun $DryRun
            }
            'performance' {
                if ($runningContainers) {
                    Collect-Performance -EnvConfig $env -IsDryRun $DryRun
                } else {
                    Write-Warning "Environment $($env.Name) is not running. Skipping performance collection."
                }
            }
            'all' {
                if ($runningContainers) {
                    Backup-Database -EnvConfig $env -IsDryRun $DryRun
                    Rotate-Logs -EnvConfig $env -IsDryRun $DryRun
                    Check-Health -EnvConfig $env -IsDryRun $DryRun
                    Collect-Performance -EnvConfig $env -IsDryRun $DryRun
                }
                Cleanup-Resources -EnvConfig $env -IsDryRun $DryRun
                Scan-Security -EnvConfig $env -IsDryRun $DryRun
            }
        }
        
        if (-not $Schedule) {
            Write-Info ""
        }
    }
    
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    if (-not $DryRun) {
        Generate-Summary -Environments $environments
    }
    
    if (-not $Schedule) {
        Write-Success "✓ Maintenance completed successfully!"
        Write-Info "Duration: $($duration.ToString('mm\:ss'))"
    }
}
catch {
    Write-Error "Maintenance failed: $($_.Exception.Message)"
    exit 1
}
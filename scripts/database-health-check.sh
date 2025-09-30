#!/bin/bash
# Comprehensive Database Health Check Script
# This script performs detailed health checks on the SQL Server container

set -e

# Configuration
DB_SERVER="${DB_SERVER:-localhost}"
DB_NAME="${DB_NAME:-QuokkaPackDb}"
SA_PASSWORD="${SA_PASSWORD:-YourStrongPassword123!}"
HEALTH_CHECK_TIMEOUT=30

echo "=== QuokkaPack Database Health Check ==="
echo "Server: $DB_SERVER"
echo "Database: $DB_NAME"
echo "Timestamp: $(date)"
echo "========================================"

# Function to run SQL command with timeout
run_sql_command() {
    local query="$1"
    local description="$2"
    
    echo -n "Checking $description... "
    
    if timeout $HEALTH_CHECK_TIMEOUT /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER" -U sa -P "$SA_PASSWORD" -Q "$query" -h -1 -W > /tmp/health_result 2>&1; then
        echo "✓ PASS"
        return 0
    else
        echo "✗ FAIL"
        cat /tmp/health_result
        return 1
    fi
}

# Function to run SQL command and capture result
run_sql_query() {
    local query="$1"
    timeout $HEALTH_CHECK_TIMEOUT /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER" -U sa -P "$SA_PASSWORD" -Q "$query" -h -1 -W 2>/dev/null || echo "ERROR"
}

# Health check results
HEALTH_CHECKS_PASSED=0
HEALTH_CHECKS_TOTAL=0

# 1. Basic connectivity test
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "SELECT 1;" "basic connectivity"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
fi

# 2. SQL Server version and edition
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "SELECT @@VERSION;" "SQL Server version"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
    echo "   Version: $(run_sql_query "SELECT SERVERPROPERTY('ProductVersion') as Version;" | head -1)"
    echo "   Edition: $(run_sql_query "SELECT SERVERPROPERTY('Edition') as Edition;" | head -1)"
fi

# 3. Database existence and status
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "SELECT name, state_desc, user_access_desc FROM sys.databases WHERE name = '$DB_NAME';" "database existence and status"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
    DB_STATE=$(run_sql_query "SELECT state_desc FROM sys.databases WHERE name = '$DB_NAME';" | head -1)
    DB_ACCESS=$(run_sql_query "SELECT user_access_desc FROM sys.databases WHERE name = '$DB_NAME';" | head -1)
    echo "   State: $DB_STATE"
    echo "   Access: $DB_ACCESS"
fi

# 4. Database size and space usage
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "USE [$DB_NAME]; SELECT 
    DB_NAME() as DatabaseName,
    SUM(CASE WHEN type = 0 THEN size END) * 8 / 1024 as DataSizeMB,
    SUM(CASE WHEN type = 1 THEN size END) * 8 / 1024 as LogSizeMB
FROM sys.master_files WHERE database_id = DB_ID();" "database size"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
fi

# 5. Table count and basic schema validation
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "USE [$DB_NAME]; SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';" "table count"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
    TABLE_COUNT=$(run_sql_query "USE [$DB_NAME]; SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';" | head -1)
    echo "   Tables: $TABLE_COUNT"
fi

# 6. Entity Framework migrations status
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "USE [$DB_NAME]; SELECT COUNT(*) as MigrationCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory';" "EF migrations table"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
    MIGRATIONS_TABLE_EXISTS=$(run_sql_query "USE [$DB_NAME]; SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory';" | head -1)
    if [ "$MIGRATIONS_TABLE_EXISTS" = "1" ]; then
        MIGRATION_COUNT=$(run_sql_query "USE [$DB_NAME]; SELECT COUNT(*) FROM [__EFMigrationsHistory];" | head -1)
        echo "   Applied migrations: $MIGRATION_COUNT"
        
        # Show latest migration
        LATEST_MIGRATION=$(run_sql_query "USE [$DB_NAME]; SELECT TOP 1 MigrationId FROM [__EFMigrationsHistory] ORDER BY MigrationId DESC;" | head -1)
        echo "   Latest migration: $LATEST_MIGRATION"
    else
        echo "   EF Migrations table not found - database may not be initialized"
    fi
fi

# 7. Connection pool and active connections
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "SELECT COUNT(*) as ActiveConnections FROM sys.dm_exec_sessions WHERE is_user_process = 1;" "active connections"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
    ACTIVE_CONNECTIONS=$(run_sql_query "SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1;" | head -1)
    echo "   Active connections: $ACTIVE_CONNECTIONS"
fi

# 8. Database performance counters
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "SELECT 
    cntr_value as BatchRequestsPerSec 
FROM sys.dm_os_performance_counters 
WHERE counter_name = 'Batch Requests/sec';" "performance counters"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
fi

# 9. Check for blocking processes
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "SELECT COUNT(*) as BlockedProcesses FROM sys.dm_exec_requests WHERE blocking_session_id > 0;" "blocking processes"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
    BLOCKED_PROCESSES=$(run_sql_query "SELECT COUNT(*) FROM sys.dm_exec_requests WHERE blocking_session_id > 0;" | head -1)
    if [ "$BLOCKED_PROCESSES" != "0" ]; then
        echo "   WARNING: $BLOCKED_PROCESSES blocked processes detected"
    fi
fi

# 10. Memory usage
HEALTH_CHECKS_TOTAL=$((HEALTH_CHECKS_TOTAL + 1))
if run_sql_command "SELECT 
    (physical_memory_kb / 1024) as PhysicalMemoryMB,
    (committed_kb / 1024) as CommittedMemoryMB,
    (committed_target_kb / 1024) as TargetMemoryMB
FROM sys.dm_os_sys_info;" "memory usage"; then
    HEALTH_CHECKS_PASSED=$((HEALTH_CHECKS_PASSED + 1))
fi

# Summary
echo ""
echo "========================================"
echo "Health Check Summary:"
echo "Passed: $HEALTH_CHECKS_PASSED/$HEALTH_CHECKS_TOTAL"

if [ $HEALTH_CHECKS_PASSED -eq $HEALTH_CHECKS_TOTAL ]; then
    echo "Status: ✓ HEALTHY"
    exit 0
elif [ $HEALTH_CHECKS_PASSED -gt $((HEALTH_CHECKS_TOTAL / 2)) ]; then
    echo "Status: ⚠ WARNING - Some checks failed"
    exit 1
else
    echo "Status: ✗ CRITICAL - Multiple checks failed"
    exit 2
fi
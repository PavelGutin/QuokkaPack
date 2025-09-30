using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using QuokkaPack.ServerCommon.Services;

namespace QuokkaPack.ServerCommon.HealthChecks;

/// <summary>
/// Health check for SQLite database with additional diagnostics
/// </summary>
public class SQLiteDatabaseHealthCheck : IHealthCheck
{
    private readonly SQLiteDatabaseService _sqliteService;
    private readonly ILogger<SQLiteDatabaseHealthCheck> _logger;

    public SQLiteDatabaseHealthCheck(SQLiteDatabaseService sqliteService, ILogger<SQLiteDatabaseHealthCheck> logger)
    {
        _sqliteService = sqliteService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var dbInfo = await _sqliteService.GetDatabaseInfoAsync(cancellationToken);
            
            var data = new Dictionary<string, object>
            {
                ["database_path"] = dbInfo.FilePath,
                ["file_size"] = dbInfo.FormattedFileSize,
                ["database_size"] = dbInfo.FormattedDatabaseSize,
                ["page_count"] = dbInfo.PageCount,
                ["page_size"] = dbInfo.PageSize,
                ["journal_mode"] = dbInfo.JournalMode,
                ["foreign_keys_enabled"] = dbInfo.ForeignKeysEnabled,
                ["last_modified"] = dbInfo.LastModified.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Check for potential issues
            var warnings = new List<string>();
            
            if (!dbInfo.ForeignKeysEnabled)
            {
                warnings.Add("Foreign key constraints are disabled");
            }
            
            if (dbInfo.JournalMode != "wal")
            {
                warnings.Add($"Journal mode is {dbInfo.JournalMode}, WAL mode recommended for better performance");
            }
            
            if (dbInfo.FileSize > 1024 * 1024 * 1024) // 1GB
            {
                warnings.Add("Database file size is quite large, consider maintenance");
            }

            if (warnings.Any())
            {
                data["warnings"] = warnings;
                return HealthCheckResult.Degraded("SQLite database is functional but has some warnings", data: data);
            }

            return HealthCheckResult.Healthy("SQLite database is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQLite health check failed");
            return HealthCheckResult.Unhealthy("SQLite database health check failed", ex);
        }
    }
}
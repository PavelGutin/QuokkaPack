using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace QuokkaPack.ServerCommon.HealthChecks;

/// <summary>
/// Comprehensive health check for API services with detailed diagnostics
/// </summary>
public class ApiHealthCheck : IHealthCheck
{
    private readonly ILogger<ApiHealthCheck> _logger;

    public ApiHealthCheck(ILogger<ApiHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly?.GetName().Version?.ToString() ?? "Unknown";
            var startTime = Environment.TickCount64;
            
            var data = new Dictionary<string, object>
            {
                ["service"] = "QuokkaPack.API",
                ["version"] = version,
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                ["machine_name"] = Environment.MachineName,
                ["process_id"] = Environment.ProcessId,
                ["uptime_ms"] = startTime,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["memory_usage_mb"] = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2),
                ["gc_collections"] = new
                {
                    gen0 = GC.CollectionCount(0),
                    gen1 = GC.CollectionCount(1),
                    gen2 = GC.CollectionCount(2)
                }
            };

            // Check memory pressure
            var memoryUsageMB = (double)data["memory_usage_mb"];
            if (memoryUsageMB > 500) // 500MB threshold
            {
                _logger.LogWarning("High memory usage detected: {MemoryUsage}MB", memoryUsageMB);
                return Task.FromResult(HealthCheckResult.Degraded("High memory usage detected", data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("API service is healthy", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("API health check failed", ex));
        }
    }
}
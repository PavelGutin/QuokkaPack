using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace QuokkaPack.ServerCommon.HealthChecks;

/// <summary>
/// Health check for web applications (Razor/Blazor) with detailed diagnostics
/// </summary>
public class WebApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<WebApplicationHealthCheck> _logger;
    private readonly string _applicationName;

    public WebApplicationHealthCheck(ILogger<WebApplicationHealthCheck> logger, string applicationName)
    {
        _logger = logger;
        _applicationName = applicationName;
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
                ["service"] = _applicationName,
                ["version"] = version,
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                ["machine_name"] = Environment.MachineName,
                ["process_id"] = Environment.ProcessId,
                ["uptime_ms"] = startTime,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["memory_usage_mb"] = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2),
                ["thread_count"] = System.Diagnostics.Process.GetCurrentProcess().Threads.Count
            };

            // Check if this is a containerized environment
            if (File.Exists("/.dockerenv"))
            {
                data["containerized"] = true;
            }

            return Task.FromResult(HealthCheckResult.Healthy($"{_applicationName} is healthy", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ApplicationName} health check failed", _applicationName);
            return Task.FromResult(HealthCheckResult.Unhealthy($"{_applicationName} health check failed", ex));
        }
    }
}
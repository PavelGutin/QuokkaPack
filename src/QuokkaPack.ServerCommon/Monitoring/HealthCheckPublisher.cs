using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace QuokkaPack.ServerCommon.Monitoring;

/// <summary>
/// Publisher for health check results to enable monitoring and alerting
/// </summary>
public class HealthCheckPublisher : IHealthCheckPublisher
{
    private readonly ILogger<HealthCheckPublisher> _logger;
    private readonly HealthCheckMetrics _metrics;

    public HealthCheckPublisher(ILogger<HealthCheckPublisher> logger, HealthCheckMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        var healthyCount = report.Entries.Count(e => e.Value.Status == HealthStatus.Healthy);
        var degradedCount = report.Entries.Count(e => e.Value.Status == HealthStatus.Degraded);
        var unhealthyCount = report.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy);

        _metrics.UpdateHealthyServicesCount(healthyCount);

        // Log overall health status
        _logger.LogInformation(
            "Health check report: Overall={OverallStatus}, Healthy={Healthy}, Degraded={Degraded}, Unhealthy={Unhealthy}, Duration={Duration}ms",
            report.Status,
            healthyCount,
            degradedCount,
            unhealthyCount,
            report.TotalDuration.TotalMilliseconds);

        // Log individual check results
        foreach (var (checkName, entry) in report.Entries)
        {
            _metrics.RecordHealthCheck(checkName, entry.Status, entry.Duration);

            if (entry.Status != HealthStatus.Healthy)
            {
                var logLevel = entry.Status == HealthStatus.Degraded ? LogLevel.Warning : LogLevel.Error;
                
                _logger.Log(logLevel,
                    "Health check {CheckName} is {Status}: {Description} (Duration: {Duration}ms)",
                    checkName,
                    entry.Status,
                    entry.Description,
                    entry.Duration.TotalMilliseconds);

                // Log additional data if available
                if (entry.Data?.Any() == true)
                {
                    _logger.LogDebug("Health check {CheckName} data: {@Data}", checkName, entry.Data);
                }

                // Log exception if present
                if (entry.Exception != null)
                {
                    _logger.LogError(entry.Exception, "Health check {CheckName} failed with exception", checkName);
                }
            }
        }

        return Task.CompletedTask;
    }
}
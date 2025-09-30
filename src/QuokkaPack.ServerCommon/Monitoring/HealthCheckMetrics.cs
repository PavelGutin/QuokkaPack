using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace QuokkaPack.ServerCommon.Monitoring;

/// <summary>
/// Metrics collection for health checks and application monitoring
/// </summary>
public class HealthCheckMetrics : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<int> _healthCheckCounter;
    private readonly Histogram<double> _healthCheckDuration;
    private readonly Gauge<int> _healthyServicesGauge;
    private readonly ILogger<HealthCheckMetrics> _logger;

    public HealthCheckMetrics(ILogger<HealthCheckMetrics> logger)
    {
        _logger = logger;
        _meter = new Meter("QuokkaPack.HealthChecks", "1.0.0");
        
        _healthCheckCounter = _meter.CreateCounter<int>(
            "health_check_total",
            description: "Total number of health checks performed");
            
        _healthCheckDuration = _meter.CreateHistogram<double>(
            "health_check_duration_seconds",
            description: "Duration of health check execution in seconds");
            
        _healthyServicesGauge = _meter.CreateGauge<int>(
            "healthy_services_count",
            description: "Number of healthy services");
    }

    /// <summary>
    /// Records health check execution metrics
    /// </summary>
    public void RecordHealthCheck(string checkName, HealthStatus status, TimeSpan duration)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("check_name", checkName),
            new("status", status.ToString().ToLowerInvariant())
        };

        _healthCheckCounter.Add(1, tags);
        _healthCheckDuration.Record(duration.TotalSeconds, tags);

        _logger.LogInformation(
            "Health check {CheckName} completed with status {Status} in {Duration}ms",
            checkName, status, duration.TotalMilliseconds);
    }

    /// <summary>
    /// Updates the count of healthy services
    /// </summary>
    public void UpdateHealthyServicesCount(int count)
    {
        _healthyServicesGauge.Record(count);
    }

    public void Dispose()
    {
        _meter?.Dispose();
    }
}
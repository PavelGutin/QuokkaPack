using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QuokkaPack.ServerCommon.Monitoring;

namespace QuokkaPack.ServerCommon.Extensions;

/// <summary>
/// Extension methods for monitoring and metrics collection
/// </summary>
public static class MonitoringExtensions
{
    /// <summary>
    /// Adds comprehensive monitoring and metrics collection for containerized deployments
    /// </summary>
    public static IServiceCollection AddContainerMonitoring(this IServiceCollection services)
    {
        // Add metrics collection
        services.AddSingleton<HealthCheckMetrics>();
        
        // Add health check publisher for monitoring
        services.AddSingleton<IHealthCheckPublisher, HealthCheckPublisher>();
        
        // Configure health check publishing
        services.Configure<HealthCheckPublisherOptions>(options =>
        {
            options.Delay = TimeSpan.FromSeconds(5);
            options.Period = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry metrics (optional, for advanced monitoring)
    /// </summary>
    public static IServiceCollection AddOpenTelemetryMetrics(this IServiceCollection services)
    {
        // This can be extended to add OpenTelemetry if needed in the future
        // For now, we're using the built-in .NET metrics system
        return services;
    }
}
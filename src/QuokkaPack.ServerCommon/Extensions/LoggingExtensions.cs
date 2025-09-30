using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;


namespace QuokkaPack.ServerCommon.Extensions;

/// <summary>
/// Extension methods for container-friendly structured logging configuration
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures container-friendly structured logging with Serilog
    /// </summary>
    public static IHostBuilder UseContainerFriendlyLogging(this IHostBuilder hostBuilder, string applicationName)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            var isContainerized = File.Exists("/.dockerenv") || 
                                 Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("Containerized", isContainerized);

            // Configure different output formats based on environment
            if (isContainerized)
            {
                // In containers, use structured JSON logging for better log aggregation
                configuration
                    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning);
            }
            else
            {
                // In development, use human-readable format
                configuration
                    .WriteTo.Console(outputTemplate: 
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .WriteTo.File(
                        path: "Logs/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        shared: true,
                        outputTemplate: 
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }

            // Add health check logging
            configuration
                .MinimumLevel.Override("Microsoft.Extensions.Diagnostics.HealthChecks", LogEventLevel.Information);
        });
    }

    /// <summary>
    /// Adds structured logging for health checks and monitoring
    /// </summary>
    public static IServiceCollection AddHealthCheckLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddFilter("Microsoft.Extensions.Diagnostics.HealthChecks", LogLevel.Information);
            builder.AddFilter("QuokkaPack.ServerCommon.HealthChecks", LogLevel.Information);
        });

        return services;
    }
}
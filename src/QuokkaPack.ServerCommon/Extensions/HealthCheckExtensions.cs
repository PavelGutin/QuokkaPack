using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using QuokkaPack.ServerCommon.HealthChecks;

namespace QuokkaPack.ServerCommon.Extensions;

/// <summary>
/// Extension methods for comprehensive health check configuration
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds comprehensive health checks for the API service
    /// </summary>
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<ApiHealthCheck>("api_service")
            .AddDbContextCheck<QuokkaPack.Data.AppDbContext>("database");

        // Add SQLite-specific health checks if using SQLite
        services.AddSQLiteHealthChecks(configuration);

        return services;
    }

    /// <summary>
    /// Adds comprehensive health checks for web applications (Razor/Blazor)
    /// </summary>
    public static IServiceCollection AddWebApplicationHealthChecks(
        this IServiceCollection services, 
        IConfiguration configuration, 
        string applicationName)
    {
        // Register the health check instances
        services.AddSingleton(sp => new WebApplicationHealthCheck(
            sp.GetRequiredService<ILogger<WebApplicationHealthCheck>>(), 
            applicationName));

        services.AddHealthChecks()
            .AddCheck<WebApplicationHealthCheck>("web_application");

        // Add external API dependency check
        var apiBaseUrl = configuration["DownstreamApi:BaseUrl"];
        if (!string.IsNullOrEmpty(apiBaseUrl))
        {
            services.AddHttpClient("HealthCheck", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            services.AddSingleton(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("HealthCheck");
                var logger = sp.GetRequiredService<ILogger<ExternalServiceHealthCheck>>();
                return new ExternalServiceHealthCheck(httpClient, logger, apiBaseUrl, "QuokkaPack.API");
            });

            services.AddHealthChecks()
                .AddCheck<ExternalServiceHealthCheck>("external_api");
        }

        return services;
    }

    /// <summary>
    /// Adds detailed health check endpoints with custom response formatting
    /// </summary>
    public static IServiceCollection AddDetailedHealthCheckEndpoints(this IServiceCollection services)
    {
        services.Configure<HealthCheckPublisherOptions>(options =>
        {
            options.Delay = TimeSpan.FromSeconds(2);
            options.Period = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
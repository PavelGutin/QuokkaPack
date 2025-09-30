using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuokkaPack.ServerCommon.Services;

namespace QuokkaPack.ServerCommon.Extensions;

/// <summary>
/// Extension methods for database-related service registration
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adds database initialization service with retry logic and automatic migrations
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDatabaseInitialization(this IServiceCollection services)
    {
        services.AddHostedService<DatabaseInitializationService>();
        return services;
    }

    /// <summary>
    /// Configures the database provider based on connection string and environment
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="logger">Optional logger for diagnostics</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddQuokkaPackDatabase(this IServiceCollection services, 
        IConfiguration configuration, ILogger? logger = null)
    {
        DatabaseConfigurationService.ConfigureDatabase(services, configuration, logger);
        
        // Register SQLite-specific services if using SQLite
        if (DatabaseConfigurationService.IsSQLiteDatabase(configuration))
        {
            services.AddScoped<SQLiteDatabaseService>();
        }
        
        return services;
    }

    /// <summary>
    /// Adds SQLite-specific health checks to the health check builder
    /// </summary>
    /// <param name="builder">The health checks builder</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The health checks builder for chaining</returns>
    public static IServiceCollection AddSQLiteHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        if (DatabaseConfigurationService.IsSQLiteDatabase(configuration))
        {
            services.AddHealthChecks()
                .AddCheck<QuokkaPack.ServerCommon.HealthChecks.SQLiteDatabaseHealthCheck>("sqlite_database");
        }
        
        return services;
    }
}
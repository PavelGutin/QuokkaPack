using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuokkaPack.Data;

namespace QuokkaPack.ServerCommon.Services;

/// <summary>
/// Service responsible for configuring database providers based on environment and connection strings
/// </summary>
public static class DatabaseConfigurationService
{
    /// <summary>
    /// Configures the appropriate database provider based on connection string and environment
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="logger">Optional logger for diagnostics</param>
    public static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration, ILogger? logger = null)
    {
        var connectionString = GetConnectionString(configuration);
        var databaseProvider = DetectDatabaseProvider(connectionString);
        
        logger?.LogInformation("Configuring database with provider: {Provider}", databaseProvider);
        logger?.LogInformation("Connection string pattern: {Pattern}", MaskConnectionString(connectionString));

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (databaseProvider)
            {
                case DatabaseProvider.SQLite:
                    ConfigureSQLite(options, connectionString, configuration, logger);
                    break;
                case DatabaseProvider.SqlServer:
                    ConfigureSqlServer(options, connectionString, logger);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported database provider: {databaseProvider}");
            }
        });
    }

    /// <summary>
    /// Gets the connection string from configuration with fallback to SQLite for self-host scenarios
    /// </summary>
    private static string GetConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback to SQLite for self-host scenarios
            var dataPath = GetSelfHostDataPath(configuration);
            connectionString = $"Data Source={Path.Combine(dataPath, "quokkapack.db")}";
        }

        return connectionString;
    }

    /// <summary>
    /// Detects the database provider based on connection string patterns
    /// </summary>
    private static DatabaseProvider DetectDatabaseProvider(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return DatabaseProvider.SQLite; // Default fallback
        }

        // SQLite patterns
        if (connectionString.Contains("Data Source=") && 
            (connectionString.Contains(".db") || connectionString.Contains(".sqlite")))
        {
            return DatabaseProvider.SQLite;
        }

        // SQL Server patterns
        if (connectionString.Contains("Server=") || 
            connectionString.Contains("Data Source=") && !connectionString.Contains(".db"))
        {
            return DatabaseProvider.SqlServer;
        }

        // Default to SQLite for self-host scenarios
        return DatabaseProvider.SQLite;
    }

    /// <summary>
    /// Configures SQLite database provider with self-host optimizations
    /// </summary>
    private static void ConfigureSQLite(DbContextOptionsBuilder options, string connectionString, 
        IConfiguration configuration, ILogger? logger)
    {
        // Ensure the database directory exists
        var dataSourceMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"Data Source=([^;]+)");
        if (dataSourceMatch.Success)
        {
            var dbPath = dataSourceMatch.Groups[1].Value;
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                logger?.LogInformation("Created database directory: {Directory}", directory);
            }
        }

        options.UseSqlite(connectionString, sqliteOptions =>
        {
            // Configure SQLite-specific options
            sqliteOptions.CommandTimeout(30);
        });

        // Enable sensitive data logging in development
        var environment = configuration["ASPNETCORE_ENVIRONMENT"];
        if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }

        logger?.LogInformation("Configured SQLite database provider");
    }

    /// <summary>
    /// Configures SQL Server database provider
    /// </summary>
    private static void ConfigureSqlServer(DbContextOptionsBuilder options, string connectionString, ILogger? logger)
    {
        options.UseSqlServer(connectionString, sqlServerOptions =>
        {
            // Configure SQL Server-specific options
            sqlServerOptions.CommandTimeout(30);
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });

        logger?.LogInformation("Configured SQL Server database provider");
    }

    /// <summary>
    /// Gets the data path for self-host scenarios
    /// </summary>
    private static string GetSelfHostDataPath(IConfiguration configuration)
    {
        // Check for explicit self-host data path
        var selfHostPath = configuration["SelfHost:DataPath"] ?? 
                          Environment.GetEnvironmentVariable("SELFHOST_DATA_PATH");
        
        if (!string.IsNullOrEmpty(selfHostPath))
        {
            return selfHostPath;
        }

        // Default to application data directory
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "QuokkaPack");
    }

    /// <summary>
    /// Masks sensitive information in connection strings for logging
    /// </summary>
    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return "Empty";
        }

        // Mask passwords and sensitive data
        var masked = System.Text.RegularExpressions.Regex.Replace(
            connectionString, 
            @"(Password|PWD|User Id|UID)=([^;]+)", 
            "$1=***", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return masked;
    }

    /// <summary>
    /// Checks if the current configuration is using SQLite
    /// </summary>
    public static bool IsSQLiteDatabase(IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);
        return DetectDatabaseProvider(connectionString) == DatabaseProvider.SQLite;
    }

    /// <summary>
    /// Checks if running in self-host mode
    /// </summary>
    public static bool IsSelfHostMode(IConfiguration configuration)
    {
        // Check for self-host environment variable or configuration
        var selfHostPath = configuration["SelfHost:DataPath"] ?? 
                          Environment.GetEnvironmentVariable("SELFHOST_DATA_PATH");
        
        if (!string.IsNullOrEmpty(selfHostPath))
        {
            return true;
        }

        // Check if using SQLite (common in self-host scenarios)
        return IsSQLiteDatabase(configuration);
    }
}

/// <summary>
/// Supported database providers
/// </summary>
public enum DatabaseProvider
{
    SqlServer,
    SQLite
}
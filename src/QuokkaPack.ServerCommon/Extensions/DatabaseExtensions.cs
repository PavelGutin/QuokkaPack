using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuokkaPack.Data;

namespace QuokkaPack.ServerCommon.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddQuokkaPackDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Detect database provider from connection string format
            if (connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                // SQLite connection string
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(connectionString);
                    // Suppress pending model changes warning for migration bundles
                    options.ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
                });
            }
            else
            {
                // SQL Server connection string
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    // Suppress pending model changes warning for migration bundles
                    options.ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
                });
            }

            return services;
        }
    }
}

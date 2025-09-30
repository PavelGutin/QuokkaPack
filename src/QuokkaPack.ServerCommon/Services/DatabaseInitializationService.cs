using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.ServerCommon.Services;

/// <summary>
/// Service responsible for initializing the database with retry logic and migrations
/// </summary>
public class DatabaseInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly IHostEnvironment _environment;

    public DatabaseInitializationService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseInitializationService> logger,
        IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database initialization...");

        try
        {
            await EnsureDatabaseAsync(cancellationToken);
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            
            // In production, we might want to fail fast
            // In development, we can be more lenient
            if (!_environment.IsDevelopment())
            {
                throw;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ensures the database exists and applies any pending migrations with retry logic
    /// </summary>
    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        const int maxRetries = 10;
        const int baseDelayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                _logger.LogInformation("Attempting to connect to database (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);

                // Test the connection
                await context.Database.CanConnectAsync(cancellationToken);
                _logger.LogInformation("Database connection successful");

                // Handle SQLite-specific initialization
                if (context.Database.IsSqlite())
                {
                    var sqliteService = scope.ServiceProvider.GetService<SQLiteDatabaseService>();
                    if (sqliteService != null)
                    {
                        await sqliteService.InitializeSQLiteDatabaseAsync(cancellationToken);
                    }
                    else
                    {
                        // Fallback to basic initialization
                        await context.Database.MigrateAsync(cancellationToken);
                    }
                }
                else
                {
                    // Check if database exists and create if necessary
                    var created = await context.Database.EnsureCreatedAsync(cancellationToken);
                    if (created)
                    {
                        _logger.LogInformation("Database created successfully");
                    }

                    // Apply any pending migrations
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                    if (pendingMigrations.Any())
                    {
                        _logger.LogInformation("Applying {Count} pending migrations: {Migrations}", 
                            pendingMigrations.Count(), 
                            string.Join(", ", pendingMigrations));
                        
                        await context.Database.MigrateAsync(cancellationToken);
                        _logger.LogInformation("Database migrations applied successfully");
                    }
                    else
                    {
                        _logger.LogInformation("No pending migrations found");
                    }
                }

                // Seed data if in development environment or self-host mode
                if (_environment.IsDevelopment() || IsSelfHostMode())
                {
                    await SeedDevelopmentDataAsync(context, cancellationToken);
                }

                return; // Success, exit retry loop
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, attempt - 1));
                _logger.LogWarning(ex, 
                    "Database initialization attempt {Attempt} failed. Retrying in {Delay}ms. Error: {Error}", 
                    attempt, delay.TotalMilliseconds, ex.Message);
                
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new InvalidOperationException($"Failed to initialize database after {maxRetries} attempts");
    }

    /// <summary>
    /// Seeds development data if the database is empty
    /// </summary>
    private async Task SeedDevelopmentDataAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Check if we already have data
            var hasData = await context.Categories.AnyAsync(cancellationToken);
            if (hasData)
            {
                _logger.LogInformation("Development data already exists, skipping seeding");
                return;
            }

            _logger.LogInformation("Seeding development data...");

            // Create a system user for default categories and items
            var systemUserId = Guid.NewGuid();
            var systemUser = new MasterUser
            {
                Id = systemUserId,
                CreatedAt = DateTime.UtcNow,
                IdentityUserId = null // System user doesn't have an identity
            };

            context.MasterUsers.Add(systemUser);

            // Add sample categories and items
            var clothingCategory = new Category
            {
                Name = "Clothing",
                IsDefault = true,
                MasterUserId = systemUserId,
                Items = new List<Item>
                {
                    new() { Name = "T-shirts", MasterUserId = systemUserId },
                    new() { Name = "Jeans", MasterUserId = systemUserId },
                    new() { Name = "Underwear", MasterUserId = systemUserId },
                    new() { Name = "Socks", MasterUserId = systemUserId }
                }
            };

            var electronicsCategory = new Category
            {
                Name = "Electronics",
                IsDefault = true,
                MasterUserId = systemUserId,
                Items = new List<Item>
                {
                    new() { Name = "Phone charger", MasterUserId = systemUserId },
                    new() { Name = "Laptop", MasterUserId = systemUserId },
                    new() { Name = "Headphones", MasterUserId = systemUserId }
                }
            };

            var toiletryCategory = new Category
            {
                Name = "Toiletries",
                IsDefault = true,
                MasterUserId = systemUserId,
                Items = new List<Item>
                {
                    new() { Name = "Toothbrush", MasterUserId = systemUserId },
                    new() { Name = "Toothpaste", MasterUserId = systemUserId },
                    new() { Name = "Shampoo", MasterUserId = systemUserId },
                    new() { Name = "Deodorant", MasterUserId = systemUserId }
                }
            };

            context.Categories.AddRange(clothingCategory, electronicsCategory, toiletryCategory);
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Development data seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to seed development data: {Error}", ex.Message);
            // Don't throw - seeding is optional
        }
    }

    /// <summary>
    /// Detects if running in self-host mode by checking for SQLite database and self-host environment variables
    /// </summary>
    private bool IsSelfHostMode()
    {
        using var scope = _serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        return DatabaseConfigurationService.IsSelfHostMode(configuration);
    }
}
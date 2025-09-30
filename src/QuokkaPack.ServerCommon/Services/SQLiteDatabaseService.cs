using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuokkaPack.Data;
using System.Data;
using Microsoft.Data.Sqlite;

namespace QuokkaPack.ServerCommon.Services;

/// <summary>
/// Service for SQLite-specific database operations and optimizations
/// </summary>
public class SQLiteDatabaseService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SQLiteDatabaseService> _logger;

    public SQLiteDatabaseService(AppDbContext context, IConfiguration configuration, ILogger<SQLiteDatabaseService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Initializes SQLite database with optimizations and proper file path management
    /// </summary>
    public async Task InitializeSQLiteDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (!_context.Database.IsSqlite())
        {
            _logger.LogWarning("InitializeSQLiteDatabaseAsync called but database is not SQLite");
            return;
        }

        try
        {
            _logger.LogInformation("Initializing SQLite database...");

            // Ensure database directory exists
            await EnsureDatabaseDirectoryAsync();

            // Apply SQLite-specific optimizations
            await ApplySQLiteOptimizationsAsync(cancellationToken);

            // Ensure database is created and migrations are applied
            await _context.Database.MigrateAsync(cancellationToken);

            // Verify database integrity
            await VerifyDatabaseIntegrityAsync(cancellationToken);

            _logger.LogInformation("SQLite database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SQLite database");
            throw;
        }
    }

    /// <summary>
    /// Ensures the database directory exists and has proper permissions
    /// </summary>
    private async Task EnsureDatabaseDirectoryAsync()
    {
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is null or empty");
        }

        // Extract database file path from connection string
        var match = System.Text.RegularExpressions.Regex.Match(connectionString, @"Data Source=([^;]+)");
        if (!match.Success)
        {
            throw new InvalidOperationException("Could not extract database path from connection string");
        }

        var dbPath = match.Groups[1].Value;
        var directory = Path.GetDirectoryName(dbPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation("Created database directory: {Directory}", directory);

            // Ensure logs directory exists as well
            var logsDirectory = Path.Combine(directory, "logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
                _logger.LogInformation("Created logs directory: {LogsDirectory}", logsDirectory);
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Applies SQLite-specific performance optimizations
    /// </summary>
    private async Task ApplySQLiteOptimizationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Applying SQLite performance optimizations...");

            // Enable foreign key constraints
            await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;", cancellationToken);

            // Set journal mode to WAL for better concurrency
            await _context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;", cancellationToken);

            // Set synchronous mode to NORMAL for better performance
            await _context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = NORMAL;", cancellationToken);

            // Set cache size (negative value means KB, positive means pages)
            await _context.Database.ExecuteSqlRawAsync("PRAGMA cache_size = -64000;", cancellationToken); // 64MB cache

            // Set temp store to memory for better performance
            await _context.Database.ExecuteSqlRawAsync("PRAGMA temp_store = MEMORY;", cancellationToken);

            // Set mmap size for memory-mapped I/O (256MB)
            await _context.Database.ExecuteSqlRawAsync("PRAGMA mmap_size = 268435456;", cancellationToken);

            _logger.LogInformation("SQLite optimizations applied successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply some SQLite optimizations, continuing anyway");
        }
    }

    /// <summary>
    /// Verifies database integrity and reports any issues
    /// </summary>
    private async Task VerifyDatabaseIntegrityAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Verifying SQLite database integrity...");

            // Run integrity check
            var integrityResult = await _context.Database
                .SqlQueryRaw<string>("PRAGMA integrity_check;")
                .FirstOrDefaultAsync(cancellationToken);

            if (integrityResult != "ok")
            {
                _logger.LogWarning("Database integrity check failed: {Result}", integrityResult);
            }
            else
            {
                _logger.LogInformation("Database integrity check passed");
            }

            // Check foreign key constraints
            var foreignKeyResult = await _context.Database
                .SqlQueryRaw<string>("PRAGMA foreign_key_check;")
                .ToListAsync(cancellationToken);

            if (foreignKeyResult.Any())
            {
                _logger.LogWarning("Foreign key constraint violations found: {Violations}", 
                    string.Join(", ", foreignKeyResult));
            }
            else
            {
                _logger.LogInformation("Foreign key constraints verified successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database integrity verification failed, but continuing");
        }
    }

    /// <summary>
    /// Backs up the SQLite database to a specified location
    /// </summary>
    public async Task BackupDatabaseAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_context.Database.IsSqlite())
        {
            throw new InvalidOperationException("Database backup is only supported for SQLite databases");
        }

        try
        {
            _logger.LogInformation("Creating database backup at: {BackupPath}", backupPath);

            var connectionString = _context.Database.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string is null or empty");
            }

            // Ensure backup directory exists
            var backupDirectory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDirectory) && !Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            // Use SQLite backup API for consistent backup
            using var sourceConnection = new SqliteConnection(connectionString);
            using var backupConnection = new SqliteConnection($"Data Source={backupPath}");
            
            await sourceConnection.OpenAsync(cancellationToken);
            await backupConnection.OpenAsync(cancellationToken);

            sourceConnection.BackupDatabase(backupConnection);

            _logger.LogInformation("Database backup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to backup database");
            throw;
        }
    }

    /// <summary>
    /// Gets database statistics and information
    /// </summary>
    public async Task<SQLiteDatabaseInfo> GetDatabaseInfoAsync(CancellationToken cancellationToken = default)
    {
        if (!_context.Database.IsSqlite())
        {
            throw new InvalidOperationException("Database info is only available for SQLite databases");
        }

        try
        {
            var connectionString = _context.Database.GetConnectionString();
            var dbPath = System.Text.RegularExpressions.Regex.Match(connectionString!, @"Data Source=([^;]+)").Groups[1].Value;
            
            var fileInfo = new FileInfo(dbPath);
            var pageCount = await _context.Database.SqlQueryRaw<long>("PRAGMA page_count;").FirstOrDefaultAsync(cancellationToken);
            var pageSize = await _context.Database.SqlQueryRaw<long>("PRAGMA page_size;").FirstOrDefaultAsync(cancellationToken);
            var journalMode = await _context.Database.SqlQueryRaw<string>("PRAGMA journal_mode;").FirstOrDefaultAsync(cancellationToken);
            var foreignKeys = await _context.Database.SqlQueryRaw<long>("PRAGMA foreign_keys;").FirstOrDefaultAsync(cancellationToken);

            return new SQLiteDatabaseInfo
            {
                FilePath = dbPath,
                FileSize = fileInfo.Exists ? fileInfo.Length : 0,
                PageCount = pageCount,
                PageSize = pageSize,
                JournalMode = journalMode ?? "unknown",
                ForeignKeysEnabled = foreignKeys == 1,
                LastModified = fileInfo.Exists ? fileInfo.LastWriteTime : DateTime.MinValue
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get database information");
            throw;
        }
    }
}

/// <summary>
/// Information about a SQLite database
/// </summary>
public class SQLiteDatabaseInfo
{
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public long PageCount { get; set; }
    public long PageSize { get; set; }
    public string JournalMode { get; set; } = string.Empty;
    public bool ForeignKeysEnabled { get; set; }
    public DateTime LastModified { get; set; }

    public long EstimatedDatabaseSize => PageCount * PageSize;
    public string FormattedFileSize => FormatBytes(FileSize);
    public string FormattedDatabaseSize => FormatBytes(EstimatedDatabaseSize);

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace QuokkaPack.ContainerTests.Infrastructure;

public class PerformanceTestHelper
{
    private readonly ILogger _logger;

    public PerformanceTestHelper(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<PerformanceMetrics> MeasureContainerStartupAsync(Func<Task<string>> startContainerFunc, 
        Func<string, Task<bool>> healthCheckFunc)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Measure container creation and start time
        var containerStartTime = Stopwatch.StartNew();
        var containerId = await startContainerFunc();
        containerStartTime.Stop();
        
        // Measure time to become healthy
        var healthCheckStartTime = Stopwatch.StartNew();
        var isHealthy = await healthCheckFunc(containerId);
        healthCheckStartTime.Stop();
        
        stopwatch.Stop();
        
        var metrics = new PerformanceMetrics
        {
            ContainerId = containerId,
            TotalStartupTime = stopwatch.Elapsed,
            ContainerStartTime = containerStartTime.Elapsed,
            HealthCheckTime = healthCheckStartTime.Elapsed,
            IsHealthy = isHealthy
        };
        
        _logger.LogInformation("Container {ContainerId} performance: Total={TotalMs}ms, Start={StartMs}ms, Health={HealthMs}ms", 
            containerId[..12], 
            metrics.TotalStartupTime.TotalMilliseconds,
            metrics.ContainerStartTime.TotalMilliseconds,
            metrics.HealthCheckTime.TotalMilliseconds);
        
        return metrics;
    }

    public async Task<ResourceUsageMetrics> MeasureResourceUsageAsync(string containerId, TimeSpan measurementDuration)
    {
        var measurements = new List<ResourceSnapshot>();
        var stopwatch = Stopwatch.StartNew();
        
        while (stopwatch.Elapsed < measurementDuration)
        {
            var snapshot = await GetResourceSnapshotAsync(containerId);
            if (snapshot != null)
            {
                measurements.Add(snapshot);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        
        if (measurements.Count == 0)
        {
            return new ResourceUsageMetrics();
        }
        
        var metrics = new ResourceUsageMetrics
        {
            AverageMemoryUsageMB = measurements.Average(m => m.MemoryUsageMB),
            PeakMemoryUsageMB = measurements.Max(m => m.MemoryUsageMB),
            AverageCpuPercent = measurements.Average(m => m.CpuPercent),
            PeakCpuPercent = measurements.Max(m => m.CpuPercent),
            MeasurementCount = measurements.Count,
            MeasurementDuration = measurementDuration
        };
        
        _logger.LogInformation("Container {ContainerId} resource usage: Avg Memory={AvgMemMB}MB, Peak Memory={PeakMemMB}MB, Avg CPU={AvgCpu}%, Peak CPU={PeakCpu}%",
            containerId[..12],
            metrics.AverageMemoryUsageMB,
            metrics.PeakMemoryUsageMB,
            metrics.AverageCpuPercent,
            metrics.PeakCpuPercent);
        
        return metrics;
    }

    private async Task<ResourceSnapshot?> GetResourceSnapshotAsync(string containerId)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"stats {containerId} --no-stream --format \"table {{{{.MemUsage}}}}\\t{{{{.CPUPerc}}}}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var process = Process.Start(processInfo);
            if (process == null) return null;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0) return null;

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) return null; // Skip header line

            var parts = lines[1].Split('\t');
            if (parts.Length < 2) return null;

            // Parse memory usage (format: "123.4MiB / 2GiB")
            var memoryPart = parts[0].Split('/')[0].Trim();
            var memoryValue = ParseMemoryValue(memoryPart);

            // Parse CPU percentage (format: "12.34%")
            var cpuPart = parts[1].Replace("%", "").Trim();
            if (!double.TryParse(cpuPart, out var cpuPercent))
            {
                cpuPercent = 0;
            }

            return new ResourceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                MemoryUsageMB = memoryValue,
                CpuPercent = cpuPercent
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get resource snapshot for container {ContainerId}", containerId[..12]);
            return null;
        }
    }

    private static double ParseMemoryValue(string memoryString)
    {
        var numericPart = new string(memoryString.Where(c => char.IsDigit(c) || c == '.').ToArray());
        if (!double.TryParse(numericPart, out var value))
        {
            return 0;
        }

        if (memoryString.Contains("GiB") || memoryString.Contains("GB"))
        {
            return value * 1024; // Convert to MB
        }
        
        if (memoryString.Contains("KiB") || memoryString.Contains("KB"))
        {
            return value / 1024; // Convert to MB
        }

        return value; // Assume MB
    }
}

public class PerformanceMetrics
{
    public string ContainerId { get; set; } = string.Empty;
    public TimeSpan TotalStartupTime { get; set; }
    public TimeSpan ContainerStartTime { get; set; }
    public TimeSpan HealthCheckTime { get; set; }
    public bool IsHealthy { get; set; }
}

public class ResourceUsageMetrics
{
    public double AverageMemoryUsageMB { get; set; }
    public double PeakMemoryUsageMB { get; set; }
    public double AverageCpuPercent { get; set; }
    public double PeakCpuPercent { get; set; }
    public int MeasurementCount { get; set; }
    public TimeSpan MeasurementDuration { get; set; }
}

public class ResourceSnapshot
{
    public DateTime Timestamp { get; set; }
    public double MemoryUsageMB { get; set; }
    public double CpuPercent { get; set; }
}
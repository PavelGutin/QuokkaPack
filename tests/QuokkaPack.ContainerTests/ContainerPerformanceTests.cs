using FluentAssertions;
using Microsoft.Extensions.Logging;
using QuokkaPack.ContainerTests.Infrastructure;

namespace QuokkaPack.ContainerTests;

public class ContainerPerformanceTests : DockerTestBase
{
    private readonly PerformanceTestHelper _performanceHelper;

    public ContainerPerformanceTests() : base(CreateLogger<ContainerPerformanceTests>())
    {
        _performanceHelper = new PerformanceTestHelper(Logger);
    }

    [Fact]
    public async Task API_Container_Startup_Performance_Should_Be_Acceptable()
    {
        // Arrange
        var imageName = "quokkapack-api-perf-test:latest";
        await BuildDockerImageAsync("src/QuokkaPack.API/Dockerfile", imageName);

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["ConnectionStrings__DefaultConnection"] = "Server=localhost;Database=QuokkaPackTest;Trusted_Connection=true;"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["8080/tcp"] = "8080"
        };

        // Act
        var metrics = await _performanceHelper.MeasureContainerStartupAsync(
            () => RunContainerAsync(imageName, environmentVariables, portMappings),
            containerId => WaitForContainerHealthyAsync(containerId, TimeSpan.FromMinutes(2))
        );

        // Assert
        metrics.TotalStartupTime.Should().BeLessThan(TimeSpan.FromMinutes(2), 
            "API container should start within 2 minutes");
        metrics.ContainerStartTime.Should().BeLessThan(TimeSpan.FromSeconds(30), 
            "Container creation should be fast");
        metrics.IsHealthy.Should().BeTrue("Container should become healthy");

        Logger.LogInformation("API container startup performance: {TotalTime}ms", 
            metrics.TotalStartupTime.TotalMilliseconds);
    }  
  [Fact]
    public async Task Razor_Container_Startup_Performance_Should_Be_Acceptable()
    {
        // Arrange
        var imageName = "quokkapack-razor-perf-test:latest";
        await BuildDockerImageAsync("src/QuokkaPack.Razor/Dockerfile", imageName);

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["8080/tcp"] = "8081"
        };

        // Act
        var metrics = await _performanceHelper.MeasureContainerStartupAsync(
            () => RunContainerAsync(imageName, environmentVariables, portMappings),
            containerId => WaitForContainerHealthyAsync(containerId, TimeSpan.FromMinutes(2))
        );

        // Assert
        metrics.TotalStartupTime.Should().BeLessThan(TimeSpan.FromMinutes(2));
        metrics.IsHealthy.Should().BeTrue();

        Logger.LogInformation("Razor container startup performance: {TotalTime}ms", 
            metrics.TotalStartupTime.TotalMilliseconds);
    }

    [Fact]
    public async Task SelfHost_Container_Startup_Performance_Should_Be_Reasonable()
    {
        // Arrange
        var imageName = "quokkapack-selfhost-perf-test:latest";
        await BuildDockerImageAsync("Dockerfile.selfhost.razor", imageName);

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["SelfHost__DataPath"] = "/app/data"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["80/tcp"] = "8096"
        };

        var volumeMappings = new Dictionary<string, string>
        {
            [Path.Combine(Path.GetTempPath(), "quokkapack-perf-data")] = "/app/data"
        };

        // Act
        var metrics = await _performanceHelper.MeasureContainerStartupAsync(
            () => RunContainerAsync(imageName, environmentVariables, portMappings, volumeMappings),
            containerId => WaitForContainerHealthyAsync(containerId, TimeSpan.FromMinutes(3))
        );

        // Assert
        metrics.TotalStartupTime.Should().BeLessThan(TimeSpan.FromMinutes(3), 
            "Self-host container should start within 3 minutes (includes DB initialization)");
        metrics.IsHealthy.Should().BeTrue();

        Logger.LogInformation("Self-host container startup performance: {TotalTime}ms", 
            metrics.TotalStartupTime.TotalMilliseconds);
    }  
  [Fact]
    public async Task Container_Resource_Usage_Should_Be_Within_Limits()
    {
        // Arrange
        var imageName = "quokkapack-api-resource-test:latest";
        await BuildDockerImageAsync("src/QuokkaPack.API/Dockerfile", imageName);

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["8080/tcp"] = "8097"
        };

        var containerId = await RunContainerAsync(imageName, environmentVariables, portMappings);
        await WaitForContainerHealthyAsync(containerId, TimeSpan.FromMinutes(2));

        // Act - Measure resource usage for 30 seconds
        var resourceMetrics = await _performanceHelper.MeasureResourceUsageAsync(
            containerId, TimeSpan.FromSeconds(30));

        // Assert
        resourceMetrics.PeakMemoryUsageMB.Should().BeLessThan(512, 
            "API container should use less than 512MB memory");
        resourceMetrics.AverageCpuPercent.Should().BeLessThan(50, 
            "API container should use less than 50% CPU on average");
        resourceMetrics.MeasurementCount.Should().BeGreaterThan(20, 
            "Should have collected sufficient measurements");

        Logger.LogInformation("Container resource usage - Memory: {MemoryMB}MB, CPU: {CpuPercent}%", 
            resourceMetrics.AverageMemoryUsageMB, resourceMetrics.AverageCpuPercent);
    }

    [Fact]
    public async Task Multiple_Containers_Should_Start_Concurrently()
    {
        // Arrange
        var apiImageName = "quokkapack-api-concurrent-test:latest";
        var razorImageName = "quokkapack-razor-concurrent-test:latest";
        
        await BuildDockerImageAsync("src/QuokkaPack.API/Dockerfile", apiImageName);
        await BuildDockerImageAsync("src/QuokkaPack.Razor/Dockerfile", razorImageName);

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
        };

        // Act - Start containers concurrently
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var apiTask = RunContainerAsync(apiImageName, environmentVariables, 
            new Dictionary<string, string> { ["8080/tcp"] = "8098" });
        var razorTask = RunContainerAsync(razorImageName, environmentVariables, 
            new Dictionary<string, string> { ["8080/tcp"] = "8099" });

        var containerIds = await Task.WhenAll(apiTask, razorTask);
        
        var healthTasks = containerIds.Select(id => 
            WaitForContainerHealthyAsync(id, TimeSpan.FromMinutes(2))).ToArray();
        var healthResults = await Task.WhenAll(healthTasks);
        
        stopwatch.Stop();

        // Assert
        healthResults.Should().AllBeEquivalentTo(true, "All containers should become healthy");
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMinutes(3), 
            "Concurrent startup should complete within 3 minutes");

        Logger.LogInformation("Concurrent container startup completed in {ElapsedTime}", stopwatch.Elapsed);
    }

    private static ILogger<T> CreateLogger<T>()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        return loggerFactory.CreateLogger<T>();
    }
}
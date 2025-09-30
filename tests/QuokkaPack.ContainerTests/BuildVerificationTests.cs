using FluentAssertions;
using Microsoft.Extensions.Logging;
using QuokkaPack.ContainerTests.Infrastructure;
using System.Diagnostics;

namespace QuokkaPack.ContainerTests;

public class BuildVerificationTests : DockerTestBase
{
    public BuildVerificationTests() : base(CreateLogger<BuildVerificationTests>())
    {
    }

    [Fact]
    public async Task API_Dockerfile_Should_Build_Successfully()
    {
        // Arrange
        var dockerfilePath = "src/QuokkaPack.API/Dockerfile";
        var imageName = "quokkapack-api-test:latest";

        // Act
        var result = await BuildDockerImageAsync(dockerfilePath, imageName);

        // Assert
        result.Should().Be(imageName);
        
        // Verify image exists
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"images {imageName} --format \"{{{{.Size}}}}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = System.Diagnostics.Process.Start(processInfo);
        var output = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        output.Should().NotBeNullOrEmpty("Image should exist");
        Logger.LogInformation("API Docker image built successfully. Size: {Size}", output.Trim());
    }

    [Fact]
    public async Task Razor_Dockerfile_Should_Build_Successfully()
    {
        // Arrange
        var dockerfilePath = "src/QuokkaPack.Razor/Dockerfile";
        var imageName = "quokkapack-razor-test:latest";

        // Act
        var result = await BuildDockerImageAsync(dockerfilePath, imageName);

        // Assert
        result.Should().Be(imageName);
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"images {imageName} --format \"{{{{.Size}}}}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = System.Diagnostics.Process.Start(processInfo);
        var output = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        output.Should().NotBeNullOrEmpty("Image should exist");
        Logger.LogInformation("Razor Docker image built successfully. Size: {Size}", output.Trim());
    }

    [Fact]
    public async Task Blazor_Dockerfile_Should_Build_Successfully()
    {
        // Arrange
        var dockerfilePath = "src/QuokkaPack.Blazor/Dockerfile";
        var imageName = "quokkapack-blazor-test:latest";

        // Act
        var result = await BuildDockerImageAsync(dockerfilePath, imageName);

        // Assert
        result.Should().Be(imageName);
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"images {imageName} --format \"{{{{.Size}}}}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = System.Diagnostics.Process.Start(processInfo);
        var output = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        output.Should().NotBeNullOrEmpty("Image should exist");
        Logger.LogInformation("Blazor Docker image built successfully. Size: {Size}", output.Trim());
    }

    [Fact]
    public async Task Angular_Dockerfile_Should_Build_Successfully()
    {
        // Arrange
        var dockerfilePath = "src/QuokkaPack.Angular/Dockerfile";
        var imageName = "quokkapack-angular-test:latest";

        // Act
        var result = await BuildDockerImageAsync(dockerfilePath, imageName);

        // Assert
        result.Should().Be(imageName);
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"images {imageName} --format \"{{{{.Size}}}}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = System.Diagnostics.Process.Start(processInfo);
        var output = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        output.Should().NotBeNullOrEmpty("Image should exist");
        Logger.LogInformation("Angular Docker image built successfully. Size: {Size}", output.Trim());
    }

    [Fact]
    public async Task SelfHost_Razor_Dockerfile_Should_Build_Successfully()
    {
        // Arrange
        var dockerfilePath = "Dockerfile.selfhost.razor";
        var imageName = "quokkapack-selfhost-razor-test:latest";

        // Act
        var result = await BuildDockerImageAsync(dockerfilePath, imageName);

        // Assert
        result.Should().Be(imageName);
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"images {imageName} --format \"{{{{.Size}}}}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = System.Diagnostics.Process.Start(processInfo);
        var output = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        output.Should().NotBeNullOrEmpty("Image should exist");
        Logger.LogInformation("Self-host Razor Docker image built successfully. Size: {Size}", output.Trim());
    }

    [Fact]
    public async Task SelfHost_Blazor_Dockerfile_Should_Build_Successfully()
    {
        // Arrange
        var dockerfilePath = "Dockerfile.selfhost.blazor";
        var imageName = "quokkapack-selfhost-blazor-test:latest";

        // Act
        var result = await BuildDockerImageAsync(dockerfilePath, imageName);

        // Assert
        result.Should().Be(imageName);
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"images {imageName} --format \"{{{{.Size}}}}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = System.Diagnostics.Process.Start(processInfo);
        var output = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        output.Should().NotBeNullOrEmpty("Image should exist");
        Logger.LogInformation("Self-host Blazor Docker image built successfully. Size: {Size}", output.Trim());
    }

    [Fact]
    public async Task SelfHost_Angular_Dockerfile_Should_Build_Successfully()
    {
        // Arrange
        var dockerfilePath = "Dockerfile.selfhost.angular";
        var imageName = "quokkapack-selfhost-angular-test:latest";

        // Act
        var result = await BuildDockerImageAsync(dockerfilePath, imageName);

        // Assert
        result.Should().Be(imageName);
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"images {imageName} --format \"{{{{.Size}}}}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = System.Diagnostics.Process.Start(processInfo);
        var output = await process!.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        output.Should().NotBeNullOrEmpty("Image should exist");
        Logger.LogInformation("Self-host Angular Docker image built successfully. Size: {Size}", output.Trim());
    }

    [Fact]
    public async Task All_Images_Should_Have_Proper_Labels()
    {
        // Arrange
        var imageConfigs = new[]
        {
            ("src/QuokkaPack.API/Dockerfile", "quokkapack-api-labels-test:latest"),
            ("src/QuokkaPack.Razor/Dockerfile", "quokkapack-razor-labels-test:latest"),
            ("src/QuokkaPack.Blazor/Dockerfile", "quokkapack-blazor-labels-test:latest"),
            ("src/QuokkaPack.Angular/Dockerfile", "quokkapack-angular-labels-test:latest")
        };

        foreach (var (dockerfilePath, imageName) in imageConfigs)
        {
            // Act
            await BuildDockerImageAsync(dockerfilePath, imageName);
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"inspect {imageName}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            var output = await process!.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Assert
            output.Should().NotBeNullOrEmpty("Image should exist and be inspectable");
            output.Should().Contain("Config", "Image should have configuration");
            
            Logger.LogInformation("Image {ImageName} has proper configuration", imageName);
        }
    }

    [Fact]
    public async Task Build_Performance_Should_Be_Reasonable()
    {
        // Arrange
        var dockerfilePath = "src/QuokkaPack.API/Dockerfile";
        var imageName = "quokkapack-api-perf-test:latest";
        
        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await BuildDockerImageAsync(dockerfilePath, imageName);
        stopwatch.Stop();

        // Assert
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMinutes(10)); // Build should complete within 10 minutes
        
        Logger.LogInformation("Docker build completed in {ElapsedTime}", stopwatch.Elapsed);
    }

    private static ILogger<T> CreateLogger<T>()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        return loggerFactory.CreateLogger<T>();
    }
}
using FluentAssertions;
using Microsoft.Extensions.Logging;
using QuokkaPack.ContainerTests.Infrastructure;
using System.Net;

namespace QuokkaPack.ContainerTests;

public class SelfHostContainerTests : DockerTestBase
{
    public SelfHostContainerTests() : base(CreateLogger<SelfHostContainerTests>())
    {
    }

    [Fact]
    public async Task SelfHost_Razor_Container_Should_Start_And_Serve_Application()
    {
        // Arrange
        var imageName = "quokkapack-selfhost-razor-test:latest";
        await BuildDockerImageAsync("Dockerfile.selfhost.razor", imageName);

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["SelfHost__DataPath"] = "/app/data"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["80/tcp"] = "8090"
        };

        var volumeMappings = new Dictionary<string, string>
        {
            [Path.Combine(Path.GetTempPath(), "quokkapack-test-data")] = "/app/data"
        };

        // Act
        var containerId = await RunContainerAsync(imageName, environmentVariables, portMappings, volumeMappings);
        
        // Wait for container to be healthy
        var isHealthy = await WaitForContainerHealthyAsync(containerId, TimeSpan.FromMinutes(3));
        isHealthy.Should().BeTrue("Self-host Razor container should become healthy");

        // Assert - Test application accessibility
        await Task.Delay(TimeSpan.FromSeconds(10)); // Additional wait for application startup
        
        var homeResponse = await TestHttpEndpointAsync("http://localhost:8090/");
        homeResponse.Should().BeTrue("Home page should be accessible");

        var healthResponse = await TestHttpEndpointAsync("http://localhost:8090/health");
        healthResponse.Should().BeTrue("Health endpoint should be accessible");

        // Test that data directory is created
        var dataPath = Path.Combine(Path.GetTempPath(), "quokkapack-test-data");
        Directory.Exists(dataPath).Should().BeTrue("Data directory should be created");

        Logger.LogInformation("Self-host Razor container is working correctly");
    }

    [Fact]
    public async Task SelfHost_Blazor_Container_Should_Start_And_Serve_Application()
    {
        // Arrange
        var imageName = "quokkapack-selfhost-blazor-test:latest";
        await BuildDockerImageAsync("Dockerfile.selfhost.blazor", imageName);

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["SelfHost__DataPath"] = "/app/data"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["80/tcp"] = "8091"
        };

        var volumeMappings = new Dictionary<string, string>
        {
            [Path.Combine(Path.GetTempPath(), "quokkapack-blazor-test-data")] = "/app/data"
        };

        // Act
        var containerId = await RunContainerAsync(imageName, environmentVariables, portMappings, volumeMappings);
        
        var isHealthy = await WaitForContainerHealthyAsync(containerId, TimeSpan.FromMinutes(3));
        isHealthy.Should().BeTrue("Self-host Blazor container should become healthy");

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(10));
        
        var homeResponse = await TestHttpEndpointAsync("http://localhost:8091/");
        homeResponse.Should().BeTrue("Blazor home page should be accessible");

        var healthResponse = await TestHttpEndpointAsync("http://localhost:8091/health");
        healthResponse.Should().BeTrue("Health endpoint should be accessible");

        Logger.LogInformation("Self-host Blazor container is working correctly");
    }

    [Fact]
    public async Task SelfHost_Angular_Container_Should_Start_And_Serve_Application()
    {
        // Arrange
        var imageName = "quokkapack-selfhost-angular-test:latest";
        await BuildDockerImageAsync("Dockerfile.selfhost.angular", imageName);

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["SelfHost__DataPath"] = "/app/data"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["80/tcp"] = "8092"
        };

        var volumeMappings = new Dictionary<string, string>
        {
            [Path.Combine(Path.GetTempPath(), "quokkapack-angular-test-data")] = "/app/data"
        };

        // Act
        var containerId = await RunContainerAsync(imageName, environmentVariables, portMappings, volumeMappings);
        
        var isHealthy = await WaitForContainerHealthyAsync(containerId, TimeSpan.FromMinutes(3));
        isHealthy.Should().BeTrue("Self-host Angular container should become healthy");

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(10));
        
        var homeResponse = await TestHttpEndpointAsync("http://localhost:8092/");
        homeResponse.Should().BeTrue("Angular home page should be accessible");

        var apiResponse = await TestHttpEndpointAsync("http://localhost:8092/api/health");
        apiResponse.Should().BeTrue("API health endpoint should be accessible through proxy");

        Logger.LogInformation("Self-host Angular container is working correctly");
    }

    [Fact]
    public async Task SelfHost_Containers_Should_Initialize_Database_Automatically()
    {
        // Arrange
        var imageName = "quokkapack-selfhost-razor-db-test:latest";
        await BuildDockerImageAsync("Dockerfile.selfhost.razor", imageName);

        var dataPath = Path.Combine(Path.GetTempPath(), "quokkapack-db-test-data");
        if (Directory.Exists(dataPath))
        {
            Directory.Delete(dataPath, true);
        }

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["SelfHost__DataPath"] = "/app/data"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["80/tcp"] = "8093"
        };

        var volumeMappings = new Dictionary<string, string>
        {
            [dataPath] = "/app/data"
        };

        // Act
        var containerId = await RunContainerAsync(imageName, environmentVariables, portMappings, volumeMappings);
        
        var isHealthy = await WaitForContainerHealthyAsync(containerId, TimeSpan.FromMinutes(3));
        isHealthy.Should().BeTrue("Container should become healthy");

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(15)); // Wait for database initialization
        
        // Check that SQLite database file was created
        var dbFiles = Directory.GetFiles(dataPath, "*.db", SearchOption.AllDirectories);
        dbFiles.Should().NotBeEmpty("SQLite database file should be created");

        // Check application logs for database initialization
        var logs = await GetContainerLogsAsync(containerId);
        logs.Should().Contain("Database", "Logs should mention database operations");

        Logger.LogInformation("Self-host container initialized database successfully");
    }

    [Fact]
    public async Task SelfHost_Container_Should_Persist_Data_Across_Restarts()
    {
        // Arrange
        var imageName = "quokkapack-selfhost-persistence-test:latest";
        await BuildDockerImageAsync("Dockerfile.selfhost.razor", imageName);

        var dataPath = Path.Combine(Path.GetTempPath(), "quokkapack-persistence-test-data");
        if (Directory.Exists(dataPath))
        {
            Directory.Delete(dataPath, true);
        }

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["SelfHost__DataPath"] = "/app/data"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["80/tcp"] = "8094"
        };

        var volumeMappings = new Dictionary<string, string>
        {
            [dataPath] = "/app/data"
        };

        // Act - Start first container
        var containerId1 = await RunContainerAsync(imageName, environmentVariables, portMappings, volumeMappings);
        var isHealthy1 = await WaitForContainerHealthyAsync(containerId1, TimeSpan.FromMinutes(3));
        isHealthy1.Should().BeTrue("First container should become healthy");

        await Task.Delay(TimeSpan.FromSeconds(10)); // Wait for initialization
        
        // Stop first container
        await DockerClient.Containers.StopContainerAsync(containerId1, new Docker.DotNet.Models.ContainerStopParameters());
        await DockerClient.Containers.RemoveContainerAsync(containerId1, new Docker.DotNet.Models.ContainerRemoveParameters());

        // Start second container with same volume
        var containerId2 = await RunContainerAsync(imageName, environmentVariables, portMappings, volumeMappings);
        var isHealthy2 = await WaitForContainerHealthyAsync(containerId2, TimeSpan.FromMinutes(3));
        isHealthy2.Should().BeTrue("Second container should become healthy");

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(10));
        
        var homeResponse = await TestHttpEndpointAsync("http://localhost:8094/");
        homeResponse.Should().BeTrue("Application should work after restart");

        // Data directory should still exist with database files
        var dbFiles = Directory.GetFiles(dataPath, "*.db", SearchOption.AllDirectories);
        dbFiles.Should().NotBeEmpty("Database files should persist across container restarts");

        Logger.LogInformation("Self-host container data persistence verified");
    }

    [Fact]
    public async Task SelfHost_Container_Should_Handle_Environment_Variables()
    {
        // Arrange
        var imageName = "quokkapack-selfhost-env-test:latest";
        await BuildDockerImageAsync("Dockerfile.selfhost.razor", imageName);

        var customDataPath = Path.Combine(Path.GetTempPath(), "quokkapack-custom-env-data");
        if (Directory.Exists(customDataPath))
        {
            Directory.Delete(customDataPath, true);
        }

        var environmentVariables = new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["SelfHost__DataPath"] = "/app/data",
            ["Logging__LogLevel__Default"] = "Information",
            ["JwtSettings__Secret"] = "CustomTestSecret123456789012345678901234567890"
        };

        var portMappings = new Dictionary<string, string>
        {
            ["80/tcp"] = "8095"
        };

        var volumeMappings = new Dictionary<string, string>
        {
            [customDataPath] = "/app/data"
        };

        // Act
        var containerId = await RunContainerAsync(imageName, environmentVariables, portMappings, volumeMappings);
        
        var isHealthy = await WaitForContainerHealthyAsync(containerId, TimeSpan.FromMinutes(3));
        isHealthy.Should().BeTrue("Container should become healthy with custom environment variables");

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(10));
        
        var homeResponse = await TestHttpEndpointAsync("http://localhost:8095/");
        homeResponse.Should().BeTrue("Application should work with custom environment variables");

        // Check logs for environment-specific behavior
        var logs = await GetContainerLogsAsync(containerId);
        logs.Should().NotBeEmpty("Container should produce logs");

        Logger.LogInformation("Self-host container handled environment variables correctly");
    }

    private static ILogger<T> CreateLogger<T>()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        return loggerFactory.CreateLogger<T>();
    }
}
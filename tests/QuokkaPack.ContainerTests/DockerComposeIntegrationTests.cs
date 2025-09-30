using FluentAssertions;
using Microsoft.Extensions.Logging;
using QuokkaPack.ContainerTests.Infrastructure;
using System.Net;

namespace QuokkaPack.ContainerTests;

public class DockerComposeIntegrationTests : IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private DockerComposeTestHelper? _composeHelper;

    public DockerComposeIntegrationTests()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        _logger = loggerFactory.CreateLogger<DockerComposeIntegrationTests>();
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
    }

    [Fact]
    public async Task Development_Environment_Should_Start_Successfully()
    {
        // Arrange
        _composeHelper = new DockerComposeTestHelper(_logger, "docker-compose.dev.yml");

        // Act
        var startResult = await _composeHelper.StartServicesAsync();

        // Assert
        startResult.Should().BeTrue("Development environment should start successfully");

        // Wait for services to become healthy
        var apiHealthy = await _composeHelper.WaitForServiceHealthyAsync("api", TimeSpan.FromMinutes(3));
        var razorHealthy = await _composeHelper.WaitForServiceHealthyAsync("razor", TimeSpan.FromMinutes(3));
        var blazorHealthy = await _composeHelper.WaitForServiceHealthyAsync("blazor", TimeSpan.FromMinutes(3));
        var angularHealthy = await _composeHelper.WaitForServiceHealthyAsync("angular", TimeSpan.FromMinutes(3));
        var dbHealthy = await _composeHelper.WaitForServiceHealthyAsync("db", TimeSpan.FromMinutes(2));

        apiHealthy.Should().BeTrue("API service should be healthy");
        razorHealthy.Should().BeTrue("Razor service should be healthy");
        blazorHealthy.Should().BeTrue("Blazor service should be healthy");
        angularHealthy.Should().BeTrue("Angular service should be healthy");
        dbHealthy.Should().BeTrue("Database service should be healthy");

        _logger.LogInformation("All development services are healthy");
    }

    [Fact]
    public async Task Production_Environment_Should_Start_Successfully()
    {
        // Arrange
        _composeHelper = new DockerComposeTestHelper(_logger, "docker-compose.prod.yml");

        // Act
        var startResult = await _composeHelper.StartServicesAsync();

        // Assert
        startResult.Should().BeTrue("Production environment should start successfully");

        // Wait for services to become healthy
        var apiHealthy = await _composeHelper.WaitForServiceHealthyAsync("api", TimeSpan.FromMinutes(3));
        var razorHealthy = await _composeHelper.WaitForServiceHealthyAsync("razor", TimeSpan.FromMinutes(3));
        var blazorHealthy = await _composeHelper.WaitForServiceHealthyAsync("blazor", TimeSpan.FromMinutes(3));
        var angularHealthy = await _composeHelper.WaitForServiceHealthyAsync("angular", TimeSpan.FromMinutes(3));
        var dbHealthy = await _composeHelper.WaitForServiceHealthyAsync("db", TimeSpan.FromMinutes(2));

        apiHealthy.Should().BeTrue("API service should be healthy");
        razorHealthy.Should().BeTrue("Razor service should be healthy");
        blazorHealthy.Should().BeTrue("Blazor service should be healthy");
        angularHealthy.Should().BeTrue("Angular service should be healthy");
        dbHealthy.Should().BeTrue("Database service should be healthy");

        _logger.LogInformation("All production services are healthy");
    }

    [Fact]
    public async Task API_Service_Should_Respond_To_Health_Checks()
    {
        // Arrange
        _composeHelper = new DockerComposeTestHelper(_logger, "docker-compose.dev.yml");
        await _composeHelper.StartServicesAsync("api", "db");
        await _composeHelper.WaitForServiceHealthyAsync("api", TimeSpan.FromMinutes(3));

        var ports = await _composeHelper.GetServicePortsAsync("api");
        var apiPort = ExtractPortNumber(ports.Values.FirstOrDefault() ?? "8080");

        // Act & Assert
        var healthResponse = await _httpClient.GetAsync($"http://localhost:{apiPort}/health");
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var swaggerResponse = await _httpClient.GetAsync($"http://localhost:{apiPort}/swagger");
        swaggerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _logger.LogInformation("API service health checks passed");
    }

    [Fact]
    public async Task Razor_Service_Should_Serve_Web_Pages()
    {
        // Arrange
        _composeHelper = new DockerComposeTestHelper(_logger, "docker-compose.dev.yml");
        await _composeHelper.StartServicesAsync("razor", "api", "db");
        await _composeHelper.WaitForServiceHealthyAsync("razor", TimeSpan.FromMinutes(3));

        var ports = await _composeHelper.GetServicePortsAsync("razor");
        var razorPort = ExtractPortNumber(ports.Values.FirstOrDefault() ?? "8081");

        // Act & Assert
        var homeResponse = await _httpClient.GetAsync($"http://localhost:{razorPort}/");
        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await homeResponse.Content.ReadAsStringAsync();
        content.Should().Contain("QuokkaPack", "Home page should contain application name");

        _logger.LogInformation("Razor service web pages are accessible");
    }

    [Fact]
    public async Task Blazor_Service_Should_Serve_Web_Application()
    {
        // Arrange
        _composeHelper = new DockerComposeTestHelper(_logger, "docker-compose.dev.yml");
        await _composeHelper.StartServicesAsync("blazor", "api", "db");
        await _composeHelper.WaitForServiceHealthyAsync("blazor", TimeSpan.FromMinutes(3));

        var ports = await _composeHelper.GetServicePortsAsync("blazor");
        var blazorPort = ExtractPortNumber(ports.Values.FirstOrDefault() ?? "8082");

        // Act & Assert
        var homeResponse = await _httpClient.GetAsync($"http://localhost:{blazorPort}/");
        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await homeResponse.Content.ReadAsStringAsync();
        content.Should().Contain("QuokkaPack", "Blazor app should contain application name");

        _logger.LogInformation("Blazor service web application is accessible");
    }

    [Fact]
    public async Task Angular_Service_Should_Serve_SPA()
    {
        // Arrange
        _composeHelper = new DockerComposeTestHelper(_logger, "docker-compose.dev.yml");
        await _composeHelper.StartServicesAsync("angular", "api", "db");
        await _composeHelper.WaitForServiceHealthyAsync("angular", TimeSpan.FromMinutes(3));

        var ports = await _composeHelper.GetServicePortsAsync("angular");
        var angularPort = ExtractPortNumber(ports.Values.FirstOrDefault() ?? "8083");

        // Act & Assert
        var homeResponse = await _httpClient.GetAsync($"http://localhost:{angularPort}/");
        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await homeResponse.Content.ReadAsStringAsync();
        content.Should().Contain("QuokkaPack", "Angular app should contain application name");

        _logger.LogInformation("Angular service SPA is accessible");
    }

    [Fact]
    public async Task Database_Service_Should_Accept_Connections()
    {
        // Arrange
        _composeHelper = new DockerComposeTestHelper(_logger, "docker-compose.dev.yml");
        await _composeHelper.StartServicesAsync("db");
        await _composeHelper.WaitForServiceHealthyAsync("db", TimeSpan.FromMinutes(2));

        // Act & Assert
        var logs = await _composeHelper.GetServiceLogsAsync("db");
        logs.Should().Contain("SQL Server is now ready for client connections", 
            "Database should be ready for connections");

        _logger.LogInformation("Database service is accepting connections");
    }

    [Fact]
    public async Task Services_Should_Communicate_With_Each_Other()
    {
        // Arrange
        _composeHelper = new DockerComposeTestHelper(_logger, "docker-compose.dev.yml");
        await _composeHelper.StartServicesAsync();
        
        // Wait for all services
        await _composeHelper.WaitForServiceHealthyAsync("api", TimeSpan.FromMinutes(3));
        await _composeHelper.WaitForServiceHealthyAsync("razor", TimeSpan.FromMinutes(3));
        await _composeHelper.WaitForServiceHealthyAsync("db", TimeSpan.FromMinutes(2));

        var apiPorts = await _composeHelper.GetServicePortsAsync("api");
        var razorPorts = await _composeHelper.GetServicePortsAsync("razor");
        
        var apiPort = ExtractPortNumber(apiPorts.Values.FirstOrDefault() ?? "8080");
        var razorPort = ExtractPortNumber(razorPorts.Values.FirstOrDefault() ?? "8081");

        // Act & Assert - Test that Razor can communicate with API
        var razorResponse = await _httpClient.GetAsync($"http://localhost:{razorPort}/");
        razorResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Test API endpoints directly
        var apiHealthResponse = await _httpClient.GetAsync($"http://localhost:{apiPort}/health");
        apiHealthResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _logger.LogInformation("Services are communicating successfully");
    }

    [Fact]
    public async Task Environment_Should_Handle_Service_Restart()
    {
        // Arrange
        _composeHelper = new DockerComposeTestHelper(_logger, "docker-compose.dev.yml");
        await _composeHelper.StartServicesAsync("api", "db");
        await _composeHelper.WaitForServiceHealthyAsync("api", TimeSpan.FromMinutes(3));

        var ports = await _composeHelper.GetServicePortsAsync("api");
        var apiPort = ExtractPortNumber(ports.Values.FirstOrDefault() ?? "8080");

        // Verify service is working
        var initialResponse = await _httpClient.GetAsync($"http://localhost:{apiPort}/health");
        initialResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Restart the API service
        await _composeHelper.StopServicesAsync();
        await _composeHelper.StartServicesAsync("api", "db");
        await _composeHelper.WaitForServiceHealthyAsync("api", TimeSpan.FromMinutes(3));

        // Assert - Service should work again
        var restartResponse = await _httpClient.GetAsync($"http://localhost:{apiPort}/health");
        restartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _logger.LogInformation("Service restart handled successfully");
    }

    private static string ExtractPortNumber(string portMapping)
    {
        // Extract port from format like "0.0.0.0:8080" or "8080"
        var parts = portMapping.Split(':');
        return parts.Length > 1 ? parts[^1] : parts[0];
    }

    public async ValueTask DisposeAsync()
    {
        if (_composeHelper != null)
        {
            await _composeHelper.DisposeAsync();
        }
        _httpClient.Dispose();
    }
}
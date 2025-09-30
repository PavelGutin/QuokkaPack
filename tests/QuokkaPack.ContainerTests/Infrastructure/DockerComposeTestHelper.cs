using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace QuokkaPack.ContainerTests.Infrastructure;

public class DockerComposeTestHelper : IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly string _composeFile;
    private readonly string _projectName;
    private readonly List<string> _startedServices = new();

    public DockerComposeTestHelper(ILogger logger, string composeFile, string? projectName = null)
    {
        _logger = logger;
        _composeFile = composeFile;
        _projectName = projectName ?? $"test-{Guid.NewGuid():N}";
    }

    public async Task<bool> StartServicesAsync(params string[] services)
    {
        var serviceList = services.Length > 0 ? string.Join(" ", services) : "";
        var command = $"up -d --build {serviceList}";
        
        var result = await RunDockerComposeCommandAsync(command);
        
        if (result.Success)
        {
            _startedServices.AddRange(services.Length > 0 ? services : await GetAllServicesAsync());
            _logger.LogInformation("Started Docker Compose services: {Services}", string.Join(", ", _startedServices));
        }
        
        return result.Success;
    }

    public async Task<bool> StopServicesAsync()
    {
        var result = await RunDockerComposeCommandAsync("down -v --remove-orphans");
        
        if (result.Success)
        {
            _logger.LogInformation("Stopped Docker Compose services");
            _startedServices.Clear();
        }
        
        return result.Success;
    }

    public async Task<bool> WaitForServiceHealthyAsync(string serviceName, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        
        while (stopwatch.Elapsed < timeout)
        {
            var result = await RunDockerComposeCommandAsync($"ps --services --filter status=running");
            
            if (result.Success && result.Output.Contains(serviceName))
            {
                // Check if service has health check
                var healthResult = await RunDockerComposeCommandAsync($"ps {serviceName}");
                if (healthResult.Success && (healthResult.Output.Contains("healthy") || !healthResult.Output.Contains("health")))
                {
                    _logger.LogInformation("Service {ServiceName} is healthy", serviceName);
                    return true;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        _logger.LogWarning("Service {ServiceName} did not become healthy within {Timeout}", serviceName, timeout);
        return false;
    }

    public async Task<string> GetServiceLogsAsync(string serviceName)
    {
        var result = await RunDockerComposeCommandAsync($"logs {serviceName}");
        return result.Output;
    }

    public async Task<Dictionary<string, string>> GetServicePortsAsync(string serviceName)
    {
        var result = await RunDockerComposeCommandAsync($"port {serviceName}");
        var ports = new Dictionary<string, string>();
        
        if (result.Success)
        {
            var lines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(" -> ");
                if (parts.Length == 2)
                {
                    ports[parts[0]] = parts[1];
                }
            }
        }
        
        return ports;
    }

    private async Task<List<string>> GetAllServicesAsync()
    {
        var result = await RunDockerComposeCommandAsync("config --services");
        return result.Success 
            ? result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList()
            : new List<string>();
    }

    private async Task<(bool Success, string Output)> RunDockerComposeCommandAsync(string command)
    {
        var fullCommand = $"-f {_composeFile} -p {_projectName} {command}";
        
        _logger.LogDebug("Running: docker-compose {Command}", fullCommand);
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker-compose",
            Arguments = fullCommand,
            WorkingDirectory = DockerTestBase.GetRepositoryRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        var output = new StringBuilder();
        var error = new StringBuilder();

        using var process = new Process { StartInfo = processInfo };
        
        process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        await process.WaitForExitAsync();

        var success = process.ExitCode == 0;
        var combinedOutput = output.ToString();
        
        if (!success)
        {
            combinedOutput += "\nErrors:\n" + error.ToString();
            _logger.LogError("Docker Compose command failed: {Command}\nOutput: {Output}", fullCommand, combinedOutput);
        }
        else
        {
            _logger.LogDebug("Docker Compose command succeeded: {Command}", fullCommand);
        }

        return (success, combinedOutput);
    }

    public async ValueTask DisposeAsync()
    {
        if (_startedServices.Count > 0)
        {
            await StopServicesAsync();
        }
    }
}
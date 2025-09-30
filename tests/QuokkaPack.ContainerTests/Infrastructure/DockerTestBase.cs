using Docker.DotNet;
using Docker.DotNet.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace QuokkaPack.ContainerTests.Infrastructure;

public abstract class DockerTestBase : IAsyncDisposable
{
    protected readonly IDockerClient DockerClient;
    protected readonly ILogger Logger;
    protected readonly List<string> CreatedContainers = new();
    protected readonly List<string> CreatedImages = new();
    protected readonly HttpClient HttpClient;

    protected DockerTestBase(ILogger logger)
    {
        Logger = logger;
        DockerClient = new DockerClientConfiguration().CreateClient();
        HttpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    }

    protected async Task<string> BuildDockerImageAsync(string dockerfilePath, string imageName, string? buildContext = null)
    {
        buildContext ??= GetRepositoryRoot();
        
        Logger.LogInformation("Building Docker image {ImageName} from {DockerfilePath}", imageName, dockerfilePath);
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"build -f {dockerfilePath} -t {imageName} {buildContext}",
            WorkingDirectory = buildContext,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = new Process { StartInfo = processInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var errorMessage = error.ToString();
            Logger.LogError("Docker build failed: {Error}", errorMessage);
            throw new InvalidOperationException($"Docker build failed: {errorMessage}");
        }

        CreatedImages.Add(imageName);
        Logger.LogInformation("Successfully built image {ImageName}", imageName);
        return imageName;
    }

    protected async Task<string> RunContainerAsync(string imageName, Dictionary<string, string>? environmentVariables = null, 
        Dictionary<string, string>? portMappings = null, Dictionary<string, string>? volumeMappings = null)
    {
        var args = new List<string> { "run", "-d" };
        
        // Add environment variables
        if (environmentVariables != null)
        {
            foreach (var env in environmentVariables)
            {
                args.Add("-e");
                args.Add($"{env.Key}={env.Value}");
            }
        }
        
        // Add port mappings
        if (portMappings != null)
        {
            foreach (var port in portMappings)
            {
                args.Add("-p");
                args.Add($"{port.Value}:{port.Key.Replace("/tcp", "")}");
            }
        }
        
        // Add volume mappings
        if (volumeMappings != null)
        {
            foreach (var volume in volumeMappings)
            {
                args.Add("-v");
                args.Add($"{volume.Key}:{volume.Value}");
            }
        }
        
        args.Add(imageName);

        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = string.Join(" ", args),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = new Process { StartInfo = processInfo };
        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Logger.LogError("Failed to start container: {Error}", error);
            throw new InvalidOperationException($"Failed to start container: {error}");
        }

        var containerId = output.Trim();
        CreatedContainers.Add(containerId);
        
        Logger.LogInformation("Started container {ContainerId} from image {ImageName}", containerId[..12], imageName);
        return containerId;
    }

    protected async Task<bool> WaitForContainerHealthyAsync(string containerId, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        
        while (stopwatch.Elapsed < timeout)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"inspect {containerId} --format=\"{{{{.State.Status}}}} {{{{if .State.Health}}}}{{{{.State.Health.Status}}}}{{{{end}}}}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                var parts = output.Trim().Split(' ');
                var status = parts[0];
                var healthStatus = parts.Length > 1 ? parts[1] : "";
                
                if (status == "exited")
                {
                    var logs = await GetContainerLogsAsync(containerId);
                    Logger.LogError("Container {ContainerId} exited. Logs: {Logs}", containerId[..12], logs);
                    return false;
                }
                
                if (healthStatus == "healthy" || (string.IsNullOrEmpty(healthStatus) && status == "running"))
                {
                    Logger.LogInformation("Container {ContainerId} is healthy", containerId[..12]);
                    return true;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        Logger.LogWarning("Container {ContainerId} did not become healthy within {Timeout}", containerId[..12], timeout);
        return false;
    }

    protected async Task<string> GetContainerLogsAsync(string containerId)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"logs {containerId}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = new Process { StartInfo = processInfo };
        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        return output + error;
    }

    private async Task RunDockerCommandAsync(string arguments)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = new Process { StartInfo = processInfo };
        process.Start();
        await process.WaitForExitAsync();
    }

    protected async Task<bool> TestHttpEndpointAsync(string url, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
    {
        try
        {
            var response = await HttpClient.GetAsync(url);
            Logger.LogInformation("HTTP GET {Url} returned {StatusCode}", url, response.StatusCode);
            return response.StatusCode == expectedStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to test HTTP endpoint {Url}", url);
            return false;
        }
    }

    public static string GetRepositoryRoot()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        while (currentDirectory != null && !File.Exists(Path.Combine(currentDirectory, "QuokkaPack.sln")))
        {
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        }
        return currentDirectory ?? throw new InvalidOperationException("Could not find repository root");
    }



    public async ValueTask DisposeAsync()
    {
        // Clean up containers
        foreach (var containerId in CreatedContainers)
        {
            try
            {
                await RunDockerCommandAsync($"stop {containerId}");
                await RunDockerCommandAsync($"rm -f {containerId}");
                Logger.LogInformation("Cleaned up container {ContainerId}", containerId[..12]);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to clean up container {ContainerId}", containerId[..12]);
            }
        }

        // Clean up images
        foreach (var imageName in CreatedImages)
        {
            try
            {
                await RunDockerCommandAsync($"rmi -f {imageName}");
                Logger.LogInformation("Cleaned up image {ImageName}", imageName);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to clean up image {ImageName}", imageName);
            }
        }

        HttpClient.Dispose();
        DockerClient.Dispose();
    }
}
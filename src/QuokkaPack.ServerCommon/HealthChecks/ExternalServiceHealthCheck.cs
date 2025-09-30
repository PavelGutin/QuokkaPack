using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace QuokkaPack.ServerCommon.HealthChecks;

/// <summary>
/// Health check for external service dependencies (like API connectivity from frontend)
/// </summary>
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;
    private readonly string _serviceUrl;
    private readonly string _serviceName;

    public ExternalServiceHealthCheck(
        HttpClient httpClient, 
        ILogger<ExternalServiceHealthCheck> logger, 
        string serviceUrl, 
        string serviceName)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceUrl = serviceUrl;
        _serviceName = serviceName;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            using var response = await _httpClient.GetAsync($"{_serviceUrl}/health", cancellationToken);
            
            stopwatch.Stop();
            
            var data = new Dictionary<string, object>
            {
                ["service_name"] = _serviceName,
                ["service_url"] = _serviceUrl,
                ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                ["status_code"] = (int)response.StatusCode,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            };

            if (response.IsSuccessStatusCode)
            {
                // Check response time
                if (stopwatch.ElapsedMilliseconds > 5000) // 5 second threshold
                {
                    _logger.LogWarning("Slow response from {ServiceName}: {ResponseTime}ms", _serviceName, stopwatch.ElapsedMilliseconds);
                    return HealthCheckResult.Degraded($"{_serviceName} is responding slowly", data: data);
                }

                return HealthCheckResult.Healthy($"{_serviceName} is healthy", data);
            }
            else
            {
                _logger.LogWarning("{ServiceName} returned status code {StatusCode}", _serviceName, response.StatusCode);
                return HealthCheckResult.Degraded($"{_serviceName} returned {response.StatusCode}", data: data);
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("Timeout connecting to {ServiceName} at {ServiceUrl}", _serviceName, _serviceUrl);
            return HealthCheckResult.Unhealthy($"{_serviceName} connection timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to {ServiceName} at {ServiceUrl}", _serviceName, _serviceUrl);
            return HealthCheckResult.Unhealthy($"{_serviceName} connection failed", ex);
        }
    }
}
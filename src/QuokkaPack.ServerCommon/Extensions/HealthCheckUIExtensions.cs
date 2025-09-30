using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace QuokkaPack.ServerCommon.Extensions;

/// <summary>
/// Extension methods for health check UI and detailed responses
/// </summary>
public static class HealthCheckUIExtensions
{
    /// <summary>
    /// Maps detailed health check endpoints with JSON responses
    /// </summary>
    public static IEndpointRouteBuilder MapDetailedHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // Basic health check endpoint
        endpoints.MapHealthChecks("/health");

        // Detailed health check with JSON response
        endpoints.MapHealthChecks("/health/detailed", new HealthCheckOptions
        {
            ResponseWriter = WriteDetailedResponse
        });

        // Ready check (all dependencies healthy)
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteDetailedResponse
        });

        // Live check (basic service availability)
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // Only basic liveness
            ResponseWriter = WriteDetailedResponse
        });

        return endpoints;
    }

    private static async Task WriteDetailedResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                data = entry.Value.Data,
                exception = entry.Value.Exception?.Message
            }).ToArray()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
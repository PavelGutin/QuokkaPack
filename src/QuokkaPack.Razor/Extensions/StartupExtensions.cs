using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace QuokkaPack.Razor.Extensions;

public static class StartupExtensions
{
    public static IApplicationBuilder UseRedirectToSetupIfNeeded(this IApplicationBuilder app, string apiBaseUrl)
    {
        // Run middleware to check API's setup status
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path;

            // Allow access to setup page, static files, etc.
            if (path.StartsWithSegments("/setup", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/css") ||
                path.StartsWithSegments("/js") ||
                path.StartsWithSegments("/lib"))
            {
                await next();
                return;
            }

            try
            {
                using var httpClient = new HttpClient();
                var statusUrl = $"{apiBaseUrl.TrimEnd('/')}/api/setup/status";
                var status = await httpClient.GetFromJsonAsync<SetupStatusResponse>(statusUrl);

                if (!status.databaseReady || !status.hasUsers)
                {
                    context.Response.Redirect("/Setup");
                    return;
                }
            }
            catch (Exception ex)
            {
                // If API unreachable or malformed, assume setup needed
                context.Response.Redirect("/Setup");
                return;
            }

            await next();
        });

        return app;
    }

    private class SetupStatusResponse
    {
        public bool databaseReady { get; set; }
        public bool hasUsers { get; set; }
    }
}

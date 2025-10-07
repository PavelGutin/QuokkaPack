using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using QuokkaPack.API;
using QuokkaPack.Data;

namespace QuokkaPack.ApiTests
{
    public class ApiTestFactory : WebApplicationFactory<Program>
    {
        // Use a unique database name per factory instance for test isolation
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            // WebApplicationFactory looks for content root based on the entry assembly location
            // We need to point it to the API project, not the test project
            var currentDir = Directory.GetCurrentDirectory();
            var baseDir = AppContext.BaseDirectory;

            // From test bin directory: tests/QuokkaPack.ApiTests/bin/Debug/net9.0
            // To API project: ../../../../src/QuokkaPack.API
            var apiProjectPath = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", "..", "src", "QuokkaPack.API"));

            if (Directory.Exists(apiProjectPath))
            {
                builder.UseContentRoot(apiProjectPath);
            }
            else
            {
                // Fallback: try from source directory if running from different location
                var altPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "src", "QuokkaPack.API"));
                builder.UseContentRoot(altPath);
            }

            builder.ConfigureTestServices(services =>
            {
                // Register InMemory database for testing (Program.cs skips SQL Server in Testing environment)
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                // Configure test authentication
                // Register both "TestAuth" and "Bearer" schemes pointing to the same TestAuthHandler
                // This allows controllers using [Authorize(AuthenticationSchemes = "Bearer")] to work in tests
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.TestScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, options => { })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Bearer", options => { });
            });
        }
    }
}

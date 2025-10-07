using Microsoft.Extensions.DependencyInjection;
using QuokkaPack.API.Services;
using QuokkaPack.Data;

using QuokkaPack.Shared.Models;
using System.Security.Claims;

namespace QuokkaPack.ApiTests;

public class TestScope : IAsyncDisposable
{
    public IServiceScope Scope { get; }
    public AppDbContext Db => Scope.ServiceProvider.GetRequiredService<AppDbContext>();
    public IUserResolver UserResolver => Scope.ServiceProvider.GetRequiredService<IUserResolver>();
    public ClaimsPrincipal Principal { get; }
    public MasterUser MasterUser { get; private set; } = null!;

    private TestScope(IServiceScope scope, ClaimsPrincipal principal)
    {
        Scope = scope;
        Principal = principal;
    }

    public static async Task<TestScope> CreateAsync(ApiTestFactory factory)
    {
        var scope = factory.Services.CreateScope();
        var principal = CreateTestPrincipal();
        var userResolver = scope.ServiceProvider.GetRequiredService<IUserResolver>();
        var masterUser = await userResolver.GetOrCreateAsync(principal);
        return new TestScope(scope, principal) { MasterUser = masterUser };
    }

    private static ClaimsPrincipal CreateTestPrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim("sub", "test-user-id"),
            new Claim("preferred_username", "test@example.com"),
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "11111111-1111-1111-1111-111111111111"),
            new Claim("iss", "https://login.microsoftonline.com/your-tenant-id/v2.0")
        }, "TestAuth"));
    }

    public async ValueTask DisposeAsync()
    {
        if (Scope is IAsyncDisposable asyncScope)
            await asyncScope.DisposeAsync();
        else
            Scope.Dispose();
    }
}

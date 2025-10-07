using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuokkaPack.API.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string TestScheme = "TestAuth";
    private readonly IUserResolver _userResolver;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserResolver userResolver)
        : base(options, logger, encoder, clock) {
        _userResolver = userResolver;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim("sub", "test-user-id"),
            new Claim("preferred_username", "test@example.com"),
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "11111111-1111-1111-1111-111111111111"),
            new Claim("iss", "https://login.microsoftonline.com/your-tenant-id/v2.0")

        };

        var identity = new ClaimsIdentity(claims, TestScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestScheme);

        var user = _userResolver.GetOrCreateAsync(principal);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

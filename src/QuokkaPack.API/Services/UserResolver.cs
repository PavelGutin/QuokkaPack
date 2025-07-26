using Microsoft.EntityFrameworkCore;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;
using System.Security.Claims;

namespace QuokkaPack.API.Services;

public class UserResolver : IUserResolver
{
    private readonly AppDbContext _db;

    public UserResolver(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MasterUser> GetOrCreateAsync(ClaimsPrincipal user)
    {
        try
        {
            var subject = user.FindFirst("sub")?.Value;
            var issuer = user.FindFirst("iss")?.Value;
            var provider = GetProvider(issuer);
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var displayName = user.Identity?.Name ?? "";

            if (subject == null || provider == null)
                throw new UnauthorizedAccessException("Missing required claims for identifying user.");

            // Check if we already know this login
            var login = await _db.AppUserLogins
                .Include(x => x.MasterUser)
                .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderUserId == subject);

            if (login != null)
                return login.MasterUser;

            // Try to map directly to MasterUserId if provided
            var masterUserIdClaim = user.FindFirst("master_user_id")?.Value;
            MasterUser? masterUser = null;

            if (Guid.TryParse(masterUserIdClaim, out var masterUserId))
            {
                masterUser = await _db.MasterUsers.FindAsync(masterUserId);
            }

            masterUser ??= new MasterUser();

            login = new AppUserLogin
            {
                Provider = provider,
                ProviderUserId = subject,
                Issuer = issuer ?? "",
                Email = email,
                DisplayName = displayName,
                MasterUser = masterUser
            };

            _db.AppUserLogins.Add(login);
            await _db.SaveChangesAsync();

            return masterUser;
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    private string? GetProvider(string? issuer)
    {
        if (issuer == null)
            return "local"; // fallback for JWTs from your own system

        if (issuer.Contains("sts.windows.net") || issuer.Contains("login.microsoftonline.com"))
            return "entra";

        if (issuer.Contains("accounts.google.com"))
            return "google";

        if (issuer.Contains("facebook.com"))
            return "facebook";

        return "unknown";
    }
}

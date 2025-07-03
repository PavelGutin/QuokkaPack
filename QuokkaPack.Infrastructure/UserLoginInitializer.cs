using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;

public class UserLoginInitializer : IClaimsTransformation
{
    private readonly AppDbContext _db;

    public UserLoginInitializer(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst("sub")?.Value;
        var issuer = principal.FindFirst("iss")?.Value;

        if (sub == null || issuer == null)
            return principal;

        var login = await _db.UserLogins
            .Include(l => l.MasterUser)
            .FirstOrDefaultAsync(l => l.ProviderUserId == sub && l.Issuer == issuer);

        if (login == null)
        {
            var masterUser = new MasterUser();
            login = new UserLogin
            {
                Provider = "entra",
                ProviderUserId = sub,
                Issuer = issuer,
                Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                DisplayName = principal.Identity?.Name ?? "",
                MasterUser = masterUser
            };

            _db.UserLogins.Add(login);
            await _db.SaveChangesAsync();
        }

        // Optionally: add MasterUserId as a claim for convenience
        var idClaim = new Claim("master_user_id", login.MasterUserId.ToString());
        var identity = (ClaimsIdentity)principal.Identity!;
        if (!principal.HasClaim(c => c.Type == "master_user_id"))
        {
            identity.AddClaim(idClaim);
        }

        return principal;
    }
}

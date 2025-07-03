using Microsoft.EntityFrameworkCore;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;
using System.Security.Claims;

namespace QuokkaPack.API.Services
{
    public class UserResolver : IUserResolver
    {
        private readonly AppDbContext _db;

        public UserResolver(AppDbContext db)
        {
            _db = db;
        }

        public async Task<MasterUser> GetOrCreateAsync(ClaimsPrincipal user)
        {
            var sub = user.FindFirst("sub")?.Value;
            var issuer = user.FindFirst("iss")?.Value;

            if (sub == null || issuer == null)
                throw new UnauthorizedAccessException("Missing sub or iss claim.");

            var login = await _db.UserLogins
                .Include(x => x.MasterUser)
                .FirstOrDefaultAsync(x => x.ProviderUserId == sub && x.Issuer == issuer);

            if (login != null)
                return login.MasterUser;

            var masterUser = new MasterUser();
            login = new UserLogin
            {
                Provider = "entra",
                ProviderUserId = sub,
                Issuer = issuer,
                Email = user.FindFirst(ClaimTypes.Email)?.Value,
                DisplayName = user.Identity?.Name ?? "",
                MasterUser = masterUser
            };

            _db.UserLogins.Add(login);
            await _db.SaveChangesAsync();

            return masterUser;
        }
    }
}

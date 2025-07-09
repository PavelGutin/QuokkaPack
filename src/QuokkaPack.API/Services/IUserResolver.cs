using QuokkaPack.Shared.Models;
using System.Security.Claims;

namespace QuokkaPack.API.Services
{
    public interface IUserResolver
    {
        Task<MasterUser> GetOrCreateAsync(ClaimsPrincipal user);
    }
}

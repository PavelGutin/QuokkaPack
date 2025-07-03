using System.Security.Claims;

namespace QuokkaPack.RazorPages.UserLogin
{
    public interface IUserLoginInitializer
    {
        Task InitializeAsync(ClaimsPrincipal user);
    }
}

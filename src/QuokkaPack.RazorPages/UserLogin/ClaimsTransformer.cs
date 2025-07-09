using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace QuokkaPack.RazorPages.UserLogin
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IUserLoginInitializer _initializer;

        public ClaimsTransformer(IUserLoginInitializer initializer)
        {
            _initializer = initializer;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            await _initializer.InitializeAsync(principal);
            return principal;
        }
    }
}

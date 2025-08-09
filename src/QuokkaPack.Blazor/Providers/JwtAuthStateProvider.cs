using Microsoft.AspNetCore.Components.Authorization;
using QuokkaPack.Blazor.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace QuokkaPack.Blazor.Providers
{
    public sealed class JwtAuthStateProvider(ITokenStore store) : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal _anon = new(new ClaimsIdentity());
        private static readonly TimeSpan AllowedSkew = TimeSpan.FromMinutes(1); // optional

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await store.GetAsync();
            if (string.IsNullOrWhiteSpace(token) || IsExpired(token))
            {
                if (!string.IsNullOrWhiteSpace(token))
                    await store.ClearAsync(); // purge stale token

                return new AuthenticationState(_anon);
            }

            var identity = BuildIdentityFromToken(token);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task SetTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || IsExpired(token))
            {
                await ClearTokenAsync();
                return;
            }

            await store.SetAsync(token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task ClearTokenAsync()
        {
            await store.ClearAsync();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private static bool IsExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                // JwtSecurityToken.ValidTo is UTC
                var nowUtc = DateTime.UtcNow;
                var validTo = jwt.ValidTo;

                // Consider small skew so we don't race the server
                return validTo <= (nowUtc + AllowedSkew.Negate());
            }
            catch
            {
                // If we can't parse, treat as expired/invalid
                return true;
            }
        }

        private static ClaimsIdentity BuildIdentityFromToken(string token)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return new ClaimsIdentity(jwt.Claims, authenticationType: "jwt");
            }
            catch
            {
                return new ClaimsIdentity();
            }
        }
    }
}

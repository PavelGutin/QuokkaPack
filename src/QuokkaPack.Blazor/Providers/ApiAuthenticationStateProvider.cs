using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace QuokkaPack.Blazor.Providers
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;

        public ApiAuthenticationStateProvider(IJSRuntime js)
        {
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "jwt");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Create ClaimsPrincipal from JWT claims
                var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "jwt", token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "jwt");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}

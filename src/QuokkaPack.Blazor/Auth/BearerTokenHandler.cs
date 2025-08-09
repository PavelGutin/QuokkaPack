using Microsoft.AspNetCore.Components.Authorization;
using QuokkaPack.Blazor.Providers;

namespace QuokkaPack.Blazor.Auth
{
    public sealed class BearerTokenHandler : DelegatingHandler
    {
        private readonly ITokenStore _store;
        private readonly JwtAuthStateProvider _auth; // inject the concrete type

        public BearerTokenHandler(ITokenStore store, AuthenticationStateProvider authStateProvider)
        {
            _store = store;
            _auth = (JwtAuthStateProvider)authStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var token = await _store.GetAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await base.SendAsync(request, ct);

            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token likely expired/invalid – clear it so UI reflects "logged out"
                await _auth.ClearTokenAsync();
            }

            return resp;
        }
    }
}

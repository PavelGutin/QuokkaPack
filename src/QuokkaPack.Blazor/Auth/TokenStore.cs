using Microsoft.JSInterop;

namespace QuokkaPack.Blazor.Auth
{
    public sealed class TokenStore(IJSRuntime js) : ITokenStore
    {
        const string Key = "qp.jwt";
        public ValueTask SetAsync(string token) => js.InvokeVoidAsync("localStorage.setItem", Key, token);
        public async Task<string?> GetAsync() => await js.InvokeAsync<string>("localStorage.getItem", Key);
        public ValueTask ClearAsync() => js.InvokeVoidAsync("localStorage.removeItem", Key);
    }
}

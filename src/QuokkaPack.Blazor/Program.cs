using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection; // AddHttpClient extension
using QuokkaPack.Blazor;
using QuokkaPack.Blazor.Auth;
using QuokkaPack.Blazor.Providers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Default client for loading app assets and same-origin requests
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// ---- Auth wiring ----
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

// ---- API client with bearer token handler ----
builder.Services.AddScoped<BearerTokenHandler>();

// TODO: move this to configuration later (e.g., builder.Configuration["DownstreamApi:BaseUrl"])
var apiBaseUrl = "https://localhost:7100";

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<BearerTokenHandler>();

await builder.Build().RunAsync();

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;

namespace QuokkaPack.Razor.Tools;

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpClient CreateClient(Action<DownstreamApiOptions> configureOptions, out string path)
    {
        var options = new DownstreamApiOptions();
        configureOptions(options);

        path = options.RelativePath ?? throw new InvalidOperationException("RelativePath must be set.");

        var client = _httpClientFactory.CreateClient("QuokkaApi");

        var jwt = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
        if (!string.IsNullOrEmpty(jwt))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        Console.WriteLine($"Calling: {client.BaseAddress}{path}");

        return client;
    }

    public async Task<T?> CallApiForUserAsync<T>(string serviceName, Action<DownstreamApiOptions> configureOptions) where T : class
    {
        var client = CreateClient(configureOptions, out var path);
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task DeleteForUserAsync(string serviceName, object? key, Action<DownstreamApiOptions> configureOptions)
    {
        var client = CreateClient(configureOptions, out var path);
        var response = await client.DeleteAsync(path);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteForUserAsync<TKey>(string serviceName, TKey key, Action<DownstreamApiOptions> configureOptions)
    {
        var client = CreateClient(configureOptions, out var path);
        var response = await client.DeleteAsync(path);
        response.EnsureSuccessStatusCode();
    }

    public async Task PostForUserAsync<TInput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions)
    {
        var client = CreateClient(configureOptions, out var path);
        var response = await client.PostAsJsonAsync(path, input);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TOutput?> PostForUserAsync<TInput, TOutput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions)
        where TOutput : class
    {
        var client = CreateClient(configureOptions, out var path);
        var response = await client.PostAsJsonAsync(path, input);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TOutput>();
    }

    public async Task PutForUserAsync<TInput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions)
    {
        var client = CreateClient(configureOptions, out var path);
        var response = await client.PutAsJsonAsync(path, input);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TOutput?> PutForUserAsync<TInput, TOutput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions)
        where TOutput : class
    {
        var client = CreateClient(configureOptions, out var path);
        var response = await client.PutAsJsonAsync(path, input);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TOutput>();
    }
}

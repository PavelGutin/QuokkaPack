using Microsoft.Identity.Abstractions;

namespace QuokkaPack.RazorPages.Tools
{
    public interface IApiService
    {
        Task<T?> CallApiForUserAsync<T>(string serviceName, Action<DownstreamApiOptions> configureOptions) where T : class;
        
        Task DeleteForUserAsync(string serviceName, object? key, Action<DownstreamApiOptions> configureOptions);
        Task DeleteForUserAsync<TKey>(string serviceName, TKey key, Action<DownstreamApiOptions> configureOptions);

        Task PostForUserAsync<TInput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions);
        Task<TOutput?> PostForUserAsync<TInput, TOutput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions) where TOutput : class;

        Task PutForUserAsync<TInput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions);
        Task<TOutput?> PutForUserAsync<TInput, TOutput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions)
            where TOutput : class;


    }

}

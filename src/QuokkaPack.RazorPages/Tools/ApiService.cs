using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace QuokkaPack.RazorPages.Tools
{

    public class ApiService : IApiService
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(IDownstreamApi downstreamApi, IHttpContextAccessor httpContextAccessor)
        {
            _downstreamApi = downstreamApi;
            _httpContextAccessor = httpContextAccessor;
        }

        [AuthorizeForScopes(ScopeKeySection = "bac7197e-bcf0-4ef6-864b-35c576fe01d8:access_as_user")]
        public async Task<T?> CallApiForUserAsync<T>(string serviceName, Action<DownstreamApiOptions> configureOptions) where T : class
        {
            //try
            //{
                return await _downstreamApi.CallApiForUserAsync<T>(serviceName, configureOptions);
            //}

            //catch (MicrosoftIdentityWebChallengeUserException)
            //{
            //    var context = _httpContextAccessor.HttpContext;
            //    if (context != null)
            //    {
            //        await context.ChallengeAsync(); // redirect to login
            //        context.Response.StatusCode = 401;
            //    }

            //    return null;
            //}
        }

        public async Task DeleteForUserAsync(string serviceName, object? key, Action<DownstreamApiOptions> configureOptions)
        {
            //try
            {
                await _downstreamApi.DeleteForUserAsync(serviceName, key, configureOptions);
            }
            //catch (MicrosoftIdentityWebChallengeUserException)
            //{
            //    var context = _httpContextAccessor.HttpContext;
            //    if (context != null)
            //    {
            //        await context.ChallengeAsync();
            //        context.Response.StatusCode = 401;
            //    }
            //}
        }

        public async Task DeleteForUserAsync<TKey>(string serviceName, TKey key, Action<DownstreamApiOptions> configureOptions)
        {
            //try
            {
                await _downstreamApi.DeleteForUserAsync(serviceName, key, configureOptions);
            }
            //catch (MicrosoftIdentityWebChallengeUserException)
            //{
            //    var context = _httpContextAccessor.HttpContext;
            //    if (context != null)
            //    {
            //        await context.ChallengeAsync();
            //        context.Response.StatusCode = 401;
            //    }
            //}
        }


        public async Task PostForUserAsync<TInput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions)
        {
            //try
            //{
                await _downstreamApi.PostForUserAsync(serviceName, input, configureOptions);
            //}
            //catch (MicrosoftIdentityWebChallengeUserException)
            //{
            //    var context = _httpContextAccessor.HttpContext;
            //    if (context != null)
            //    {
            //        await context.ChallengeAsync();
            //        context.Response.StatusCode = 401;
            //    }
            //}
        }
        public async Task<TOutput?> PostForUserAsync<TInput, TOutput>(
            string serviceName,
            TInput input,
            Action<DownstreamApiOptions> configureOptions) where TOutput : class
        {
            //try
            {
                return await _downstreamApi.PostForUserAsync<TInput, TOutput>(serviceName, input, configureOptions);
            }
            //catch (MicrosoftIdentityWebChallengeUserException)
            //{
            //    var context = _httpContextAccessor.HttpContext;
            //    if (context != null)
            //    {
            //        await context.ChallengeAsync();
            //        context.Response.StatusCode = 401;
            //    }

            //    return null;
            //}
        }

        public async Task PutForUserAsync<TInput>(string serviceName, TInput input, Action<DownstreamApiOptions> configureOptions)
        {
            //try
            {
                await _downstreamApi.PutForUserAsync(serviceName, input, configureOptions);
            }
            //catch (MicrosoftIdentityWebChallengeUserException)
            //{
            //    var context = _httpContextAccessor.HttpContext;
            //    if (context != null)
            //    {
            //        await context.ChallengeAsync();
            //        context.Response.StatusCode = 401;
            //    }
            //}
        }

        public async Task<TOutput?> PutForUserAsync<TInput, TOutput>(
            string serviceName,
            TInput input,
            Action<DownstreamApiOptions> configureOptions) where TOutput : class
        {
            //try
            {
                return await _downstreamApi.PutForUserAsync<TInput, TOutput>(serviceName, input, configureOptions);
            }
            //catch (MicrosoftIdentityWebChallengeUserException)
            //{
            //    var context = _httpContextAccessor.HttpContext;
            //    if (context != null)
            //    {
            //        await context.ChallengeAsync();
            //        context.Response.StatusCode = 401;
            //    }

            //    return null;
            //}
        }



    }
}

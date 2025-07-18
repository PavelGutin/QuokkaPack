using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;

namespace QuokkaPack.RazorPages.Pages
{
    [AllowAnonymous]
    //[AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly IApiService _api;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IApiService api)
        {
            _logger = logger;
            _api = api; ;
        }

        public async Task OnGet()
        {
            ViewData["ApiResult"] = "Hello There";
            //using var response = await _downstreamApi
            //    .CallApiForUserAsync(
            //    "DownstreamApi", 
            //    options =>
            //    {
            //        options.RelativePath = "WeatherForecast"; 
            //    })
            //    .ConfigureAwait(false); 

            //if (response.StatusCode == System.Net.HttpStatusCode.OK)
            //{
            //    var apiResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //    ViewData["ApiResult"] = apiResult;
            //}
            //else
            //{
            //    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //    throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}: {error}");
            //}
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;

namespace QuokkaPack.RazorPages.Pages
{
    [AllowAnonymous]
    //[AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IDownstreamApi downstreamApi)
        {
            _logger = logger;
            _downstreamApi = downstreamApi; ;
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

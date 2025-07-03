using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using QuokkaPack.Data.Models;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class IndexModel : PageModel
    {
        private readonly IDownstreamApi _api;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IDownstreamApi _downstreamApi;

        public IndexModel(IDownstreamApi api, ITokenAcquisition tokenAcquisition, IDownstreamApi downstreamApi)
        {
            _api = api;
            _tokenAcquisition = tokenAcquisition;
            _downstreamApi = downstreamApi; ;
        }

        public IList<Trip> Trips { get; set; } = [];

        public async Task OnGetAsync()
        {
            Trips = await _downstreamApi.CallApiForUserAsync<IList<Trip>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/trips"
            );

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

            ////var result = await _api.CallApiForUserAsync<IList<Trip>>("QuokkaApi", options =>
            ////{
            ////    options.RelativePath = "trips";
            ////}, User);

            //Trips = result ?? [];
        }
    }
}
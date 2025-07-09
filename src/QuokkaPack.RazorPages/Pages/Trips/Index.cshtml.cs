using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Data.Models;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class IndexModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;

        public IndexModel(IDownstreamApi downstreamApi)
        {
            _downstreamApi = downstreamApi; ;
        }

        public IList<Trip> Trips { get; set; } = [];

        public async Task OnGetAsync()
        {
            Trips = await _downstreamApi.CallApiForUserAsync<IList<Trip>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/trips"
            ) ?? [];
        }
    }
}
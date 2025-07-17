using Microsoft.AspNetCore.Mvc.RazorPages;
using QuokkaPack.Data.Models;
using QuokkaPack.RazorPages.Tools;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _api;

        public IndexModel(IApiService api)
        {
            _api = api;
        }

        public IList<Trip> Trips { get; set; } = [];

        public async Task OnGetAsync()
        {
            Trips = await _api.CallApiForUserAsync<IList<Trip>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/trips"
            ) ?? [];
        }
    }
}
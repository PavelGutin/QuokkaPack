using Microsoft.AspNetCore.Mvc.RazorPages;

using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.Trip;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _api;

        public IndexModel(IApiService api)
        {
            _api = api;
        }

        public IList<TripReadDto> Trips { get; set; } = [];

        public async Task OnGetAsync()
        {
            Trips = await _api.CallApiForUserAsync<IList<TripReadDto>>(
                "DownstreamApi",
                options => options.RelativePath = "api/Trips"
            ) ?? [];
        }
    }
}
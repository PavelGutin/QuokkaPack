using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.Trip;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class DetailsModel : PageModel
    {
        private readonly IApiService _api;
        private readonly ILogger<DeleteModel> _logger;

        public DetailsModel(IApiService api, ILogger<DeleteModel> logger)
        {
            _api = api;
            _logger = logger;
        }

        public TripSummaryReadDto Trip { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var trip = await _api.CallApiForUserAsync<TripSummaryReadDto>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/Trips/{id}");

                if (trip == null)
                    return NotFound();

                Trip = trip;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trip with ID {TripId}", id);
                return NotFound();
            }
        }
    }
}

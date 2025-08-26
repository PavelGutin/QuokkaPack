using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;

using QuokkaPack.Shared.DTOs.Trip;
using System.Net;
using Microsoft.Identity.Web;
using QuokkaPack.RazorPages.Tools;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class DeleteModel : PageModel
    {
        private readonly IApiService _api;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IApiService api, ILogger<DeleteModel> logger)
        {
            _api = api;
            _logger = logger;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                await _api.DeleteForUserAsync<int>(
                    "DownstreamApi",
                    id.Value,
                    options =>
                    {
                        options.RelativePath = $"/api/Trips/{id}";

                    });

                return RedirectToPage("./Index");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when deleting trip {TripId}", id);
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting trip {TripId}", id);
                return StatusCode(500);
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.DTOs.Trip;
using System.Net;
using Microsoft.Identity.Web;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class DeleteModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IDownstreamApi downstreamApi, ILogger<DeleteModel> logger)
        {
            _downstreamApi = downstreamApi;
            _logger = logger;
        }

        [BindProperty]
        public TripReadDto Trip { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var trip = await _downstreamApi.CallApiForUserAsync<TripReadDto>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/trips/{id}");

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
                await _downstreamApi.DeleteForUserAsync<int>(
                    "DownstreamApi",
                    id.Value,
                    options =>
                    {
                        options.RelativePath = $"/api/trips/{id}";

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
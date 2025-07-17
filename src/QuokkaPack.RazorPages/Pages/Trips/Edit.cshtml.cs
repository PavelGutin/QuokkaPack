using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.Trip;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class EditModel : PageModel
    {
        private readonly IApiService _api;
        private readonly ILogger<DeleteModel> _logger;

        public EditModel(IApiService api, ILogger<DeleteModel> logger)
        {
            _api = api;
            _logger = logger;
        }

        [BindProperty]
        public TripEditDto Trip { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var trip = await _api.CallApiForUserAsync<TripEditDto>(
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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                await _api.PutForUserAsync(
                    "DownstreamApi",
                    Trip,
                    options =>
                    {
                        options.RelativePath = $"/api/trips/{Trip.Id}";
                    });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when updating trip {TripId}", Trip.Id);
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating trip {TripId}", Trip.Id);
                return StatusCode(500);
            }

            return RedirectToPage("./Index");
        }
    }
}

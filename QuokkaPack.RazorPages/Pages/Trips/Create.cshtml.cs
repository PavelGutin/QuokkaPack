using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Shared.DTOs.Trip;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class CreateModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;

        public CreateModel(IDownstreamApi downstreamApi)
        {
            _downstreamApi = downstreamApi;
        }

        [BindProperty]
        public TripCreateDto Trip { get; set; } = default!;

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _downstreamApi.PostForUserAsync(
                "DownstreamApi",
                Trip,
                options =>
                {
                    options.RelativePath = "/api/trips";
                });


            return RedirectToPage("./Index");
        }
    }
}

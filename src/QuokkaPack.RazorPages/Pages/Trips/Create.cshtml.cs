using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;

using QuokkaPack.Shared.DTOs.Trip;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class CreateModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        public List<Category> AllCategories { get; set; } = [];
        [BindProperty]
        public TripCreateDto Trip { get; set; } = default!;

        [BindProperty]
        public List<int> SelectedCategoryIds { get; set; } = [];

        public CreateModel(IDownstreamApi downstreamApi)
        {
            _downstreamApi = downstreamApi;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            AllCategories = await _downstreamApi.CallApiForUserAsync<List<Category>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/categories"
            ) ?? [];

            SelectedCategoryIds = AllCategories
                .Where(c => c.IsDefault)
                .Select(c => c.Id)
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Trip.CategoryIds = SelectedCategoryIds;

            var response = await _downstreamApi.PostForUserAsync<TripCreateDto, TripReadDto>(
                "DownstreamApi",
                Trip,
                options =>
                {
                    options.RelativePath = "/api/trips";
                });

            //TODO: Handle response errors
            return RedirectToPage("./EditTripItems", new { tripId = response.Id });
        }
    }
}

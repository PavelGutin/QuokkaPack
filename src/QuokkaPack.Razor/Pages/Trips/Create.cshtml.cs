using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.Trip;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class CreateModel : PageModel
    {
        private readonly IApiService _api;
        public List<Category> AllCategories { get; set; } = [];
        [BindProperty]
        public TripCreateDto Trip { get; set; } = default!;

        [BindProperty]
        public List<int> SelectedCategoryIds { get; set; } = [];

        public CreateModel(IApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            AllCategories = await _api.CallApiForUserAsync<List<Category>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/Categories"
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

            var response = await _api.PostForUserAsync<TripCreateDto, TripSummaryReadDto>(
                "DownstreamApi",
                Trip,
                options =>
                {
                    options.RelativePath = "/api/Trips";
                });

            //TODO: Handle response errors
            return RedirectToPage("./Edit", new { tripId = response.Id });
        }
    }
}

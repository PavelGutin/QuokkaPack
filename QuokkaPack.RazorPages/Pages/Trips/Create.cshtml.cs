using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Data.Models;

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
        public Trip Trip { get; set; } = default!;

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

            await _downstreamApi.PostForUserAsync<Trip>(
                "DownstreamApi",
                Trip,
                options =>
                {
                    options.RelativePath = "/api/trips";
                });

            //await _downstreamApi.CallApiForUserAsync(
            //    "DownstreamApi",
            //    options =>
            //    {
            //        options.RelativePath = "/api/trips";
            //        options.HttpMethod = HttpMethod.Post.ToString();
            //        options.JsonBody = Trip;
            //    });

            return RedirectToPage("./Index");
        }
    }
}

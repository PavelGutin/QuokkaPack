using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.CategoryDTOs;

namespace QuokkaPack.RazorPages.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly IApiService _api;

        public CreateModel(IApiService api)
        {
            _api = api;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public CategoryCreateDto Category { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _api.PostForUserAsync(
                "DownstreamApi",
                Category,
                options =>
                {
                    options.RelativePath = "/api/categories";
                });


            return RedirectToPage("./Index");
        }
    }
}

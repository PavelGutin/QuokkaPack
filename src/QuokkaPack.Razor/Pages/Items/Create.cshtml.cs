using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.ItemDTOs;

namespace QuokkaPack.RazorPages.Pages.Items
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
        public ItemCreateDto Item { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _api.PostForUserAsync(
                "DownstreamApi",
                Item,
                options =>
                {
                    options.RelativePath = "/api/Items";
                });


            return RedirectToPage("./Index");
        }
    }
}

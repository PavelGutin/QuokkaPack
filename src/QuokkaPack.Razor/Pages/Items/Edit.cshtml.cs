using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.ItemDTOs;

namespace QuokkaPack.RazorPages.Pages.Items
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
        public ItemEditDto Item { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var item = await _api.CallApiForUserAsync<ItemEditDto>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/Items/{id}");

                if (item == null)
                    return NotFound();

                Item = item;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item with ID {ItemId}", id);
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
                    Item,
                    options =>
                    {
                        options.RelativePath = $"/api/Categories/{Item.Id}";
                    });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when updating item {ItemId}", Item.Id);
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating item {ItemId}", Item.Id);
                return StatusCode(500);
            }

            return RedirectToPage("./Index");
        }
    }
}

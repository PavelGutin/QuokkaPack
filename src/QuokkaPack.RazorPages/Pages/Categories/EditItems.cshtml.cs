using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.DTOs.TripItem;

namespace QuokkaPack.RazorPages.Pages.Categories
{
    public class EditItemsModel : PageModel
    {
        private readonly IApiService _api;
        private readonly ILogger<EditItemsModel> _logger;

        public EditItemsModel(IApiService api, ILogger<EditItemsModel> logger)
        {
            _api = api;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int CategoryId { get; set; }

        public List<ItemReadDto> ExistingItems { get; set; } = [];

        [BindProperty]
        public ItemCreateDto NewItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var items = await _api.CallApiForUserAsync<List<ItemReadDto>>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/categories/{CategoryId}/items");

                ExistingItems = items ?? [];
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load items for category {CategoryId}", CategoryId);
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(NewItem.Name))
            {
                return await OnGetAsync();
            }

            try
            {
                var createdItem = await _api.PostForUserAsync<ItemCreateDto, ItemReadDto>(
                    "DownstreamApi",
                    NewItem,
                    options => options.RelativePath = "/api/items");

                await _api.PostForUserAsync<object>(
                    "DownstreamApi",
                    null,
                    options => options.RelativePath = $"/api/categories/{CategoryId}/items/{createdItem.Id}");

                return RedirectToPage("EditItems", new { CategoryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item to category {CategoryId}", CategoryId);
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int itemId)
        {
            try
            {
                await _api.DeleteForUserAsync(
                    "DownstreamApi",
                    itemId,
                    options => options.RelativePath = $"/api/categories/{CategoryId}/items/{itemId}");

                return RedirectToPage("EditItems", new { CategoryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete item {ItemId} from category {CategoryId}", itemId, CategoryId);
                return StatusCode(500);
            }
        }
    }
}

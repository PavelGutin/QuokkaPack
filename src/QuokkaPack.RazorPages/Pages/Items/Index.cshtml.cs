using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.CategoryDTOs;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.RazorPages.Pages.Items
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _api;

        public IndexModel(IApiService api)
        {
            _api = api; ;
        }

        public IList<Item> Items { get;set; } = default!;
        public IList<CategoryReadDto> Categories { get; set; } = default!;

        public async Task OnGetAsync()
        {
            //TODO: replace with DTOs. Do this everywhere.
            Items = await _api.CallApiForUserAsync<IList<Item>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/items"
            ) ?? [];

            Categories = await _api.CallApiForUserAsync<IList<CategoryReadDto>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/categories"
                ) ?? [];
        }

        public async Task<IActionResult> OnPostAddCategoryAsync(string categoryName, bool isDefault)
        {
            await _api.PostForUserAsync(
                "DownstreamApi",
                new CategoryCreateDto() { Name = categoryName, IsDefault = isDefault},
                options =>
                {
                    options.RelativePath = "/api/categories";
                });


            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostAddItemAsync(string itemName, bool isEssential, int categoryId)
        {
            var dto = new ItemCreateDto
            {
                Name = itemName,
                IsEssential = isEssential,
                CategoryId = categoryId
            };

            await _api.PostForUserAsync(
                "DownstreamApi",
                new ItemCreateDto() { Name = itemName, IsEssential = isEssential, CategoryId = categoryId},
                options =>
                {
                    options.RelativePath = "/api/items";
                });

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteItemAsync(int itemId)
        {
            try
            {
                await _api.DeleteForUserAsync(
                    "DownstreamApi",
                    itemId,
                    options => options.RelativePath = $"/api/items/{itemId}");

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Failed to delete item {ItemId} from Trip {TripId}", itemId, tripId);
                return StatusCode(500);
            }
        }
    }
}

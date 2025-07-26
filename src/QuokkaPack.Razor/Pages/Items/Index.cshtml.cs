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
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IApiService api, ILogger<IndexModel> logger)
        {
            _api = api;
            _logger = logger;
        }

        public IList<Item> Items { get;set; } = default!;
        public IList<CategoryReadDto> Categories { get; set; } = default!;

        public async Task OnGetAsync()
        {
            //TODO: replace with DTOs. Do this everywhere.
            Items = await _api.CallApiForUserAsync<IList<Item>>(
                "DownstreamApi",
                options => options.RelativePath = "api/Items"
            ) ?? [];

            Categories = await _api.CallApiForUserAsync<IList<CategoryReadDto>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/Categories"
                ) ?? [];
        }

        public async Task<IActionResult> OnPostAddCategoryAsync(string categoryName, string isDefault)
        {
            await _api.PostForUserAsync(
                "DownstreamApi",
                new CategoryCreateDto() { Name = categoryName, IsDefault = (isDefault == "on")}, //TODO: Figure out how to actually use a bool
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
                    options => options.RelativePath = $"/api/Items/{itemId}");

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete item {ItemId}", itemId);
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(int categoryId)
        {
            try
            {
                await _api.DeleteForUserAsync(
                    "DownstreamApi",
                    categoryId,
                    options => options.RelativePath = $"/api/Categories/{categoryId}");

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category {categoryId}", categoryId);
                return StatusCode(500);
            }
        }



        
    }
}

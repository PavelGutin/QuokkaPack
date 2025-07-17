using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.CategoryDTOs;

namespace QuokkaPack.RazorPages.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly IApiService _api;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IApiService api, ILogger<DeleteModel> logger)
        {
            _api = api;
            _logger = logger;
        }

        [BindProperty]
        public CategoryReadDto Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var category = await _api.CallApiForUserAsync<CategoryReadDto>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/categories/{id}");

                if (category == null)
                    return NotFound();

                Category = category;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category with ID {CategoryId}", id);
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                await _api.DeleteForUserAsync(
                    "DownstreamApi",
                    id.Value,
                    options =>
                    {
                        options.RelativePath = $"/api/categories/{id}";

                    });

                return RedirectToPage("./Index");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when deleting category {CategoryId}", id);
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting category {CategoryId}", id);
                return StatusCode(500);
            }
        }
    }
}

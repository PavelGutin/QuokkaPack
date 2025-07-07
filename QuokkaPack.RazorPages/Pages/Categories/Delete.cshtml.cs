using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Shared.DTOs.CategoryDTOs;

namespace QuokkaPack.RazorPages.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IDownstreamApi downstreamApi, ILogger<DeleteModel> logger)
        {
            _downstreamApi = downstreamApi;
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
                var category = await _downstreamApi.CallApiForUserAsync<CategoryReadDto>(
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
                await _downstreamApi.DeleteForUserAsync(
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

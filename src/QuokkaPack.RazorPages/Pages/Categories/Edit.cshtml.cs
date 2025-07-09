using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Shared.DTOs.CategoryDTOs;

namespace QuokkaPack.RazorPages.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly ILogger<DeleteModel> _logger;

        public EditModel(IDownstreamApi downstreamApi, ILogger<DeleteModel> logger)
        {
            _downstreamApi = downstreamApi;
            _logger = logger;
        }

        [BindProperty]
        public CategoryEditDto Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var category = await _downstreamApi.CallApiForUserAsync<CategoryEditDto>(
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
                await _downstreamApi.PutForUserAsync(
                    "DownstreamApi",
                    Category,
                    options =>
                    {
                        options.RelativePath = $"/api/categories/{Category.Id}";
                    });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when updating category {CategoryId}", Category.Id);
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating category {CategoryId}", Category.Id);
                return StatusCode(500);
            }

            return RedirectToPage("./Index");
        }
    }
}

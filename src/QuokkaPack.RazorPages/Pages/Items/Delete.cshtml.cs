using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Data;
using QuokkaPack.Shared.DTOs.CategoryDTOs;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuokkaPack.RazorPages.Pages.Items
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
        public ItemReadDto Item { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var item = await _downstreamApi.CallApiForUserAsync<ItemReadDto>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/items/{id}");

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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                await _downstreamApi.DeleteForUserAsync<int>(
                    "DownstreamApi",
                    id.Value,
                    options =>
                    {
                        options.RelativePath = $"/api/items/{id}";

                    });

                return RedirectToPage("./Index");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when deleting item {ItemId}", id);
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting item {ItemId}", id);
                return StatusCode(500);
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Data;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuokkaPack.RazorPages.Pages.Items
{
    public class CreateModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;

        public CreateModel(IDownstreamApi downstreamApi)
        {
            _downstreamApi = downstreamApi;
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

            await _downstreamApi.PostForUserAsync(
                "DownstreamApi",
                Item,
                options =>
                {
                    options.RelativePath = "/api/items";
                });


            return RedirectToPage("./Index");
        }
    }
}

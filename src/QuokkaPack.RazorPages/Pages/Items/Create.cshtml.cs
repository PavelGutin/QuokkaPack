using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Data;
using QuokkaPack.RazorPages.Tools;
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
                    options.RelativePath = "/api/items";
                });


            return RedirectToPage("./Index");
        }
    }
}

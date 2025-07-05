using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Data;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.DTOs.Category;
using QuokkaPack.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuokkaPack.RazorPages.Pages.Categories
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
        public CategoryCreateDto Category { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _downstreamApi.PostForUserAsync(
                "DownstreamApi",
                Category,
                options =>
                {
                    options.RelativePath = "/api/categories";
                });


            return RedirectToPage("./Index");
        }
    }
}

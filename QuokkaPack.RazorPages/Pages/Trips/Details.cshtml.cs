using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.Data;
using QuokkaPack.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public DetailsModel(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("QuokkaApi");
        }

        public Trip Trip { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var trip = await _context.Trips.FirstOrDefaultAsync(m => m.Id == id);
            Trip trip = null;

            if (trip is not null)
            {
                Trip = trip;

                return Page();
            }

            return NotFound();
        }
    }
}

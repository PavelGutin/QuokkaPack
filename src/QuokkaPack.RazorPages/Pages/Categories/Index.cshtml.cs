using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.RazorPages.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _api;

        public IndexModel(IApiService api)
        {
            _api = api;
        }

        public IList<Category> Categories { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Categories = await _api.CallApiForUserAsync<IList<Category>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/categories"
            ) ?? [];
        }
    }
}

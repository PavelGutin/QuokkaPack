using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.RazorPages.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;

        public IndexModel(IDownstreamApi downstreamApi)
        {
            _downstreamApi = downstreamApi; ;
        }

        public IList<Category> Categories { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Categories = await _downstreamApi.CallApiForUserAsync<IList<Category>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/categories"
            ) ?? [];
        }
    }
}

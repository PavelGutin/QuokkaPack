using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.RazorPages.Pages.Items
{
    public class IndexModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;

        public IndexModel(IDownstreamApi downstreamApi)
        {
            _downstreamApi = downstreamApi; ;
        }

        public IList<Item> Items { get;set; } = default!;

        public async Task OnGetAsync()
        {
            //TODO: replace with DTOs. Do this everywhere.
            Items = await _downstreamApi.CallApiForUserAsync<IList<Item>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/items"
            ) ?? [];
        }
    }
}

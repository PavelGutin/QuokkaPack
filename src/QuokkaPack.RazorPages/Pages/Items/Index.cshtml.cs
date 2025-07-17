using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.RazorPages.Pages.Items
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _api;

        public IndexModel(IApiService api)
        {
            _api = api; ;
        }

        public IList<Item> Items { get;set; } = default!;

        public async Task OnGetAsync()
        {
            //TODO: replace with DTOs. Do this everywhere.
            Items = await _api.CallApiForUserAsync<IList<Item>>(
                "DownstreamApi",
                options => options.RelativePath = "/api/items"
            ) ?? [];
        }
    }
}

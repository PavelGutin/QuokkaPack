using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.DTOs.TripItem;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class EditTripItemsModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly ILogger<EditTripItemsModel> _logger;

        public EditTripItemsModel(IDownstreamApi downstreamApi, ILogger<EditTripItemsModel> logger)
        {
            _downstreamApi = downstreamApi;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int TripId { get; set; }

        public List<TripItemReadDto> ExistingItems { get; set; } = [];

        [BindProperty]
        public TripItemCreateDto NewTripItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var items = await _downstreamApi.CallApiForUserAsync<List<TripItemReadDto>>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/trips/{TripId}/items");

                ExistingItems = items ?? [];
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load items for Trip {TripId}", TripId);
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(NewTripItem.Name))
            {
                return await OnGetAsync();
            }

            try
            {
                var createdItem = await _downstreamApi.PostForUserAsync<TripItemCreateDto, ItemReadDto>(
                    "DownstreamApi",
                    NewTripItem,
                    options => options.RelativePath = "/api/items");

                await _downstreamApi.PostForUserAsync<object>(
                    "DownstreamApi",
                    null,
                    options => options.RelativePath = $"/api/trips/{TripId}/items/{createdItem.Id}");

                return RedirectToPage("EditItems", new { TripId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item to Trip {TripId}", TripId);
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int itemId)
        {
            try
            {
                await _downstreamApi.DeleteForUserAsync(
                    "DownstreamApi",
                    itemId,
                    options => options.RelativePath = $"/api/trips/{TripId}/items/{itemId}");

                return RedirectToPage("EditItems", new { TripId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete item {ItemId} from Trip {TripId}", itemId, TripId);
                return StatusCode(500);
            }
        }
    }
}

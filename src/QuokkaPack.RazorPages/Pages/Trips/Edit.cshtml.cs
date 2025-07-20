using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.CategoryDTOs;
using QuokkaPack.Shared.DTOs.Trip;
using QuokkaPack.Shared.DTOs.TripItem;

namespace QuokkaPack.RazorPages.Pages.Trips
{
    public class EditModel : PageModel
    {
        private readonly IApiService _api;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IApiService api, ILogger<EditModel> logger)
        {
            _api = api;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public List<TripItemReadDto> ExistingItems { get; set; } = [];
        public List<CategoryReadDto> AllCategories { get; set; } = [];

        public TripReadDto Trip { get; set; }
        [BindProperty]
        public List<TripItemReadDto> UpdatedItems { get; set; } = [];
        [BindProperty]
        public TripItemCreateDto NewTripItem { get; set; } = new();


        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                //TODO: Convert to use a single API call that returns both trip and items
                //TODO: Factor out the API name so it's not hardcoded
                var items = await _api.CallApiForUserAsync<List<TripItemReadDto>>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/trips/{Id}/tripItems");

                var trip = await _api.CallApiForUserAsync<TripReadDto>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/trips/{Id}");

                var categories = await _api.CallApiForUserAsync<List<CategoryReadDto>>(
                    "DownstreamApi",
                    options => options.RelativePath = $"/api/categories");

                if (trip is null)
                    return NotFound();

                Trip = trip;
                ExistingItems = items ?? [];
                AllCategories = categories ?? [];
                return Page();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load items for Trip {TripId}", Id);
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostUpdatePackedStatusAsync()
        {
            try
            {
                await _api.PutForUserAsync<List<TripItemReadDto>, object>(
                    "DownstreamApi",
                    UpdatedItems,
                    options => options.RelativePath = $"/api/trips/{Id}/tripItems/batch");

                return RedirectToPage("EditTripItems", new { Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update packed status for Trip {TripId}", Id);
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            throw new NotImplementedException();
            /*
            if (!ModelState.IsValid)
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

                return RedirectToPage("EditTripItems", new { TripId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item to Trip {TripId}", TripId);
                return StatusCode(500);
            }
            */
        }

        //public async Task<IActionResult> OnPostDeleteAsync(int itemId)
        //{
        //    throw new NotImplementedException();
        //    /*
        //    try
        //    {
        //        await _downstreamApi.DeleteForUserAsync(
        //            "DownstreamApi",
        //            itemId,
        //            options => options.RelativePath = $"/api/trips/{TripId}/items/{itemId}");

        //        return RedirectToPage("EditTripItems", new { TripId });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to delete item {ItemId} from Trip {TripId}", itemId, TripId);
        //        return StatusCode(500);
        //    }
        //    */
        //}


        public async Task<IActionResult> OnPostDeleteItemAsync(int tripId, int itemId)
        {
            try
            {
                await _api.DeleteForUserAsync(
                    "DownstreamApi",
                    itemId,
                    options => options.RelativePath = $"/api/trips/{tripId}/tripItems/{itemId}");

                return RedirectToPage("Edit", new { id = tripId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete item {ItemId} from Trip {TripId}", itemId, tripId );
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> OnPostAddItemAsync(int tripId, int itemId)
        {
            try
            {
                await _api.PostForUserAsync(
                    "DownstreamApi",
                    new TripItemCreateDto() { ItemId = itemId, IsPacked = false},
                    options => options.RelativePath = $"/api/trips/{tripId}/tripItems");

                return RedirectToPage("Edit", new { id = tripId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item {ItemId} from Trip {TripId}", itemId, tripId);
                return StatusCode(500);
            }
        }
    }
}

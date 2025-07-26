using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.Shared.DTOs.CategoryDTOs;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.DTOs.Trip;
using QuokkaPack.Shared.DTOs.TripItem;
using QuokkaPack.Shared.Models;

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
        public TripReadDto Trip { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
        [BindProperty]
        public List<TripItemReadDto> ExistingItems { get; set; } = [];
        [BindProperty]
        public TripItemCreateDto NewTripItem { get; set; } = new();
        [BindProperty]
        public TripEditDto TripEdit { get; set; } = new();

        public List<CategoryReadDto> AllCategories { get; set; } = [];
        public List<ItemReadDto> AllItems { get; set; } = [];

        public List<TripItemReadDto> UpdatedItems { get; set; } = [];


        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                //TODO: Convert to use a single API call that returns both trip and items
                //TODO: Factor out the API name so it's not hardcoded
                var existingItems = await _api.CallApiForUserAsync<List<TripItemReadDto>>(
                    "DownstreamApi",
                    options => options.RelativePath = $"api/Trips/{Id}/tripItems");

                var allItems = await _api.CallApiForUserAsync<List<ItemReadDto>>(
                    "DownstreamApi",
                    options => options.RelativePath = $"api/Items");

                var trip = await _api.CallApiForUserAsync<TripReadDto>(
                    "DownstreamApi",
                    options => options.RelativePath = $"api/Trips/{Id}");

                var categories = await _api.CallApiForUserAsync<List<CategoryReadDto>>(
                    "DownstreamApi",
                    options => options.RelativePath = $"api/Categories");


                if (trip is null)
                    return NotFound();

                Trip = trip;
                ExistingItems = existingItems ?? [];
                AllCategories = categories ?? [];
                AllItems = allItems ?? [];
                return Page();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load items for Trip {TripId}", Id);
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostUpdateTripAsync(int tripId, TripEditDto trip)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                trip.Id = tripId; //TODO: Fix this, I shouldn't have to do this nonsense 
                await _api.PutForUserAsync(
                    "DownstreamApi",
                    trip,
                    options =>
                    {
                        options.RelativePath = $"/api/trips/{tripId}";
                    });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when updating trip {TripId}", tripId);
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating trip {TripId}", tripId);
                return StatusCode(500);
            }

            return RedirectToPage("./Index");
        }
        public async Task<IActionResult> OnPostDeleteItemAsync(int tripId, int itemId)
        {
            try
            {
                await _api.DeleteForUserAsync(
                    "DownstreamApi",
                    itemId,
                    options => options.RelativePath = $"/api/Trips/{tripId}/TripItems/{itemId}");

                return RedirectToPage("Edit", new { id = tripId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete item {ItemId} from Trip {TripId}", itemId, tripId );
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(int tripId, int categoryId)
        {
            try
            {
                await _api.DeleteForUserAsync(
                    "DownstreamApi",
                    categoryId,
                    options => options.RelativePath = $"/api/Trips/{tripId}/Categories/{categoryId}");

                return RedirectToPage("Edit", new { id = tripId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category {categoryId} from Trip {TripId}", categoryId, tripId);
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
                    options => options.RelativePath = $"/api/Trips/{tripId}/TripItems");

                return RedirectToPage("Edit", new { id = tripId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item {ItemId} from Trip {TripId}", itemId, tripId);
                return StatusCode(500);
            }
        }
        public async Task<IActionResult> OnPostAddCategoryAsync(int tripId, int categoryId)
        {
            try
            {
                await _api.PostForUserAsync(
                    "DownstreamApi",
                    categoryId,
                    options => options.RelativePath = $"api/Trips/{tripId}/Categories");

                return RedirectToPage("Edit", new { id = tripId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add category {categoryId} from Trip {tripId}", categoryId, tripId);
                return StatusCode(500);
            }
        }
    }
}

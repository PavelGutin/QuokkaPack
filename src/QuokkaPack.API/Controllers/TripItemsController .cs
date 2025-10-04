using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Shared.DTOs.TripItem;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/Trips/{tripId}/TripItems")]
    public class TripItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserResolver _userResolver;

        public TripItemsController(AppDbContext context, IUserResolver userResolver)
        {
            _context = context;
            _userResolver = userResolver;
        }

        /// <summary>
        /// GET /api/trips/{tripId}/tripitems - Get all items in trip
        /// Note: Usually you'd use GET /api/trips/{id} instead
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TripItemReadDto>>> GetTripItems(int tripId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var tripExists = await _context.Trips
                .AnyAsync(t => t.Id == tripId && t.MasterUserId == user.Id);

            if (!tripExists)
                return NotFound();

            var items = await _context.TripItems
                .Where(ti => ti.TripId == tripId)
                .Include(ti => ti.Item)
                    .ThenInclude(i => i.Category)
                .AsNoTracking()
                .Select(ti => new TripItemReadDto
                {
                    TripItemId = ti.Id,
                    ItemId = ti.ItemId,
                    ItemName = ti.Item.Name,
                    CategoryId = ti.Item.CategoryId,
                    CategoryName = ti.Item.Category.Name,
                    IsPacked = ti.IsPacked
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// POST /api/trips/{tripId}/tripitems - Add item to trip
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddItemToTrip(int tripId, [FromBody] TripItemCreateDto tripItemDto)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var tripExists = await _context.Trips
                .AnyAsync(t => t.Id == tripId && t.MasterUserId == user.Id);

            if (!tripExists)
                return NotFound("Trip not found");

            var itemExists = await _context.Items
                .AnyAsync(i => i.Id == tripItemDto.ItemId && i.MasterUserId == user.Id);

            if (!itemExists)
                return NotFound("Item not found");

            // Check for duplicates
            var alreadyExists = await _context.TripItems
                .AnyAsync(ti => ti.TripId == tripId && ti.ItemId == tripItemDto.ItemId);

            if (alreadyExists)
                return Conflict("Item already in trip");

            var newTripItem = new TripItem
            {
                TripId = tripId,
                ItemId = tripItemDto.ItemId,
                IsPacked = tripItemDto.IsPacked
            };

            _context.TripItems.Add(newTripItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// PUT /api/trips/{tripId}/tripitems/{tripItemId} - Update packed status
        /// </summary>
        [HttpPut("{tripItemId}")]
        public async Task<IActionResult> UpdateTripItem(int tripId, int tripItemId, [FromBody] TripItemEditDto tripItemDto)
        {
            if (tripItemId != tripItemDto.Id)
                return BadRequest("ID mismatch");

            var user = await _userResolver.GetOrCreateAsync(User);

            var tripItem = await _context.TripItems
                .Include(ti => ti.Trip)
                .FirstOrDefaultAsync(ti => ti.Id == tripItemId && ti.TripId == tripId);

            if (tripItem == null)
                return NotFound();

            if (tripItem.Trip.MasterUserId != user.Id)
                return Forbid();

            tripItem.IsPacked = tripItemDto.IsPacked;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// PUT /api/trips/{tripId}/tripitems/batch - Batch update packed status
        /// </summary>
        [HttpPut("batch")]
        public async Task<IActionResult> UpdateTripItems(int tripId, [FromBody] List<TripItemEditDto> tripItemDtos)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .Include(t => t.TripItems)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.MasterUserId == user.Id);

            if (trip == null)
                return NotFound();

            var tripItemDict = trip.TripItems.ToDictionary(ti => ti.Id);

            foreach (var dto in tripItemDtos)
            {
                if (tripItemDict.TryGetValue(dto.Id, out var tripItem))
                    tripItem.IsPacked = dto.IsPacked;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// DELETE /api/trips/{tripId}/tripitems/{tripItemId} - Remove item from trip
        /// </summary>
        [HttpDelete("{tripItemId}")]
        public async Task<IActionResult> RemoveItemFromTrip(int tripId, int tripItemId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var tripItem = await _context.TripItems
                .Include(ti => ti.Trip)
                .FirstOrDefaultAsync(ti => ti.Id == tripItemId && ti.TripId == tripId);

            if (tripItem == null)
                return NotFound();

            if (tripItem.Trip.MasterUserId != user.Id)
                return Forbid();

            _context.TripItems.Remove(tripItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

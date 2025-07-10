using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.DTOs.TripItem;
using QuokkaPack.Shared.Mappings;

namespace QuokkaPack.API.Controllers
{
    [Authorize]
    [ApiController]
    public class TripItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserResolver _userResolver;

        public TripItemsController(AppDbContext context, IUserResolver userResolver)
        {
            _context = context;
            _userResolver = userResolver;
        }


        // GET: /api/trips/{tripId}/items
        [HttpGet("api/trips/{tripId}/tripItems")]
        public async Task<ActionResult<List<TripItemReadDto>>> GetItemsForTrip(int tripId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .Include(c => c.TripItems)
                .FirstOrDefaultAsync(c => c.Id == tripId && c.MasterUserId == user.Id);

            if (trip == null)
                return NotFound();

            var items = trip.TripItems.Select(i => new TripItemReadDto
            {
                Id = i.Item.Id,
                ItemReadDto = i.Item.ToReadDto(),
                IsPacked = i.IsPacked,
            }).ToList();

            return Ok(items);
        }


        [HttpPost("api/trips/{tripId}/tripItems/{tripItemId}")]
        public async Task<IActionResult> AddItemToTrip(int tripId, [FromBody] TripItemCreateDto tripItemDto)
        {
            if (tripItemDto.TripId != tripId)
                return BadRequest("Trip ID mismatch");

            var user = await _userResolver.GetOrCreateAsync(User);

            //TODO: the AI slop code has masterUserID here, but that seems redundant. Be sure to test with multiple users
            var trip = await _context.Trips.FirstOrDefaultAsync(trip => trip.Id == tripId);
            var item = await _context.Items.FirstOrDefaultAsync(item => item.Id == tripItemDto.ItemReadDto.Id);

            if (trip == null || item == null)
                return NotFound();

            if (!trip.TripItems.Any(tripItem => tripItem.Item.Id == item.Id))  //don't add duplicates
                trip.TripItems.Add(new TripItem() { Item = item, IsPacked = false});

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: /api/trips/5/items/10
        [HttpDelete("api/trips/{tripId}/tripItems/{tripItemId}")]
        public async Task<IActionResult> RemoveItemFromTrip(int tripId, int tripItemId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips.FirstOrDefaultAsync(trip => trip.Id == tripId);

            if (trip == null)
                return NotFound();

            var tripItem = trip.TripItems.FirstOrDefault(i => i.Id == tripItemId);
            if (tripItem == null)
                return NotFound();

            trip.TripItems.Remove(tripItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // PUT: /api/trips/{tripId}/items/{itemId}
        [HttpPut("api/trips/{tripId}/tripItems/{tripItemId}")]
        public async Task<IActionResult> UpdateTripItem(int tripId, int tripItemId, [FromBody] TripItemEditDto tripItemDto)
        {
            if (tripItemDto.TripId != tripId || tripItemDto.Id != tripItemId)
                return BadRequest("Trip ID or TripItem ID mismatch");

            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips.FirstOrDefaultAsync(trip => trip.Id == tripId);

            if (trip == null)
                return NotFound();

            var tripItem = trip.TripItems.FirstOrDefault(tripItem => tripItem.Id == tripItemId);
            if (tripItem == null)
                return NotFound();

            //TODO: There might be other properties to update, so eventually move the logic to some exteion method
            tripItem.IsPacked = tripItemDto.IsPacked;

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}

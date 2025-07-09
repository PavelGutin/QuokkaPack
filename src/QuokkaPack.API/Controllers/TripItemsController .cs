using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.DTOs.ItemDTOs;
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
        [HttpGet("api/trips/{tripId}/items")]
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


        // POST: /api/trips/5/items/10
        [HttpPost("api/trips/{tripId}/items/{itemId}")]
        [HttpPost("api/items/{itemId}/trips/{tripId}")]
        public async Task<IActionResult> AddItemToTrip(int tripId, int itemId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .Include(c => c.TripItems)
                .FirstOrDefaultAsync(c => c.Id == tripId && c.MasterUserId == user.Id);
            var item = await _context.Items
                .Include(i => i.Trips)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.MasterUserId == user.Id);

            if (trip == null || item == null)
                return NotFound();

            if (!trip.TripItems.Any(i => i.Item.Id == itemId))
                trip.TripItems.Add(new TripItem() { Item = item, IsPacked = false});

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: /api/trips/5/items/10
        [HttpDelete("api/trips/{tripId}/items/{itemId}")]
        [HttpDelete("api/items/{itemId}/trips/{tripId}")]
        public async Task<IActionResult> RemoveItemFromTrip(int tripId, int itemId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .Include(c => c.TripItems)
                .FirstOrDefaultAsync(c => c.Id == tripId && c.MasterUserId == user.Id);

            if (trip == null)
                return NotFound();

            var item = trip.TripItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return NotFound();

            trip.TripItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

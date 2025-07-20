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
    [Route("api/trips/{tripId}/tripItems")]
    public class TripItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserResolver _userResolver;

        public TripItemsController(AppDbContext context, IUserResolver userResolver)
        {
            _context = context;
            _userResolver = userResolver;
        }


        // GET: /api/trips/{tripId}/tripItems
        [HttpGet()]
        public async Task<ActionResult<List<TripItemReadDto>>> GetTripItems(int tripId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .Include(trip => trip.TripItems)
                .ThenInclude(TripItem => TripItem.Item)
                .ThenInclude(item => item.Categories)
                .FirstOrDefaultAsync(c => c.Id == tripId && c.MasterUserId == user.Id);

            if (trip == null)
                return NotFound();

            var items = trip.TripItems.Select(i => new TripItemReadDto
            {
                Id = i.Id,
                ItemReadDto = i.Item.ToReadDto(),
                IsPacked = i.IsPacked,
            }).ToList();

            return Ok(items);
        }

        // GET: /api/trips/{tripId}/tripItems{tripItemId}
        [HttpGet("{tripItemId}")]
        public async Task<ActionResult<List<TripItemReadDto>>> GetTripItem(int tripId, int tripItemId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips.FirstOrDefaultAsync(trip => trip.Id == tripId);
            if (trip == null) return NotFound();

            var tripItem = trip.TripItems.FirstOrDefault(tripItem => tripItem.Id == tripItemId);
            if (tripItem == null) return NotFound();

            var result = new TripItemReadDto
            {
                Id = tripItem.Id,
                ItemReadDto = tripItem.Item.ToReadDto(),
                IsPacked = tripItem.IsPacked
            };

            return Ok(result);
        }

        //POST: /api/trips/{tripId}/tripItems
        [HttpPost()]
        public async Task<IActionResult> AddItemToTrip(int tripId, [FromBody] TripItemCreateDto tripItemDto)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            //TODO: the AI slop code has masterUserID here, but that seems redundant. Be sure to test with multiple users
            var trip = await _context.Trips
                .Include(trip => trip.TripItems)
                .ThenInclude(tripItem => tripItem.Item)
                .FirstOrDefaultAsync(trip => trip.Id == tripId);
            var item = await _context.Items.FirstOrDefaultAsync(item => item.Id == tripItemDto.ItemId);

            if (trip == null || item == null)
                return NotFound();

            //don't add duplicates
            if (!trip.TripItems.Any(tripItem => tripItem.Item.Id == item.Id))
            {
                var tripItem = new TripItem()
                {
                    Item = item,
                    IsPacked = tripItemDto.IsPacked
                };
                trip.TripItems.Add(tripItem);
                await _context.SaveChangesAsync();

                var result = new TripItemReadDto
                {
                    Id = tripItem.Id, // You'll need to track this
                    ItemReadDto = item.ToReadDto(),
                    IsPacked = tripItemDto.IsPacked
                };
                return CreatedAtAction(nameof(GetTripItem), new { tripId, tripItemId = tripItem.Id }, result);
            }
            return NoContent();
            //return Conflict(new
            //{
            //    Message = $"Item with ID {item.Id} is already part of Trip {trip.Id}."
            //});
        }

        // DELETE: /api/trips/{tripId}/tripItems/{itemId}
        [HttpDelete("{tripItemId}")]
        public async Task<IActionResult> RemoveItemFromTrip(int tripId, int tripItemId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips.FirstOrDefaultAsync(trip => trip.Id == tripId);
            if (trip == null) return NotFound();

            var tripItem = await _context.TripItems.FirstOrDefaultAsync(tripItem => tripItem.Id == tripItemId);
            if (tripItem == null) return NotFound();

            trip.TripItems.Remove(tripItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: /api/trips/{tripId}/tripItems/{itemId}
        [HttpPut("{tripItemId}")]
        public async Task<IActionResult> UpdateTripItem(int tripId, int tripItemId, [FromBody] TripItemEditDto tripItemDto)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips.FirstOrDefaultAsync(trip => trip.Id == tripId);
            if (trip == null) return BadRequest();

            var tripItem = await _context.TripItems.FirstOrDefaultAsync(tripItem => tripItem.Id == tripItemId);
            if (tripItem == null) return BadRequest();

            if (tripItemDto.Id != tripItemId) return BadRequest();

            tripItem.IsPacked = tripItemDto.IsPacked;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: /api/trips/{tripId}/tripItems/batch
        [HttpPut("batch")]
        public async Task<IActionResult> UpdateTripItems(int tripId, [FromBody] List<TripItemEditDto> tripItemDtos)
        {
            var user = await _userResolver.GetOrCreateAsync(User);
            var trip = await _context.Trips.Include(t => t.TripItems).FirstOrDefaultAsync(t => t.Id == tripId && t.MasterUserId == user.Id);
            if (trip == null) return BadRequest();

            var tripItemDict = trip.TripItems.ToDictionary(i => i.Id);
            foreach (var dto in tripItemDtos)
            {
                if (tripItemDict.TryGetValue(dto.Id, out var tripItem))
                    tripItem.IsPacked = dto.IsPacked;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Shared.DTOs.Trip;
using QuokkaPack.Shared.DTOs.TripItem;
using QuokkaPack.Shared.Mappings;

namespace QuokkaPack.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly IUserResolver _userResolver;
        private readonly AppDbContext _context;

        public TripsController(IUserResolver userResolver, AppDbContext context)
        {
            _userResolver = userResolver;
            _context = context;
        }

        /// <summary>
        /// GET /api/trips - List all trips (lightweight summary)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripSummaryReadDto>>> GetTrips()
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trips = await _context.Trips
                .Where(trip => trip.MasterUserId == user.Id)
                .AsNoTracking()
                .Select(trip => new TripSummaryReadDto
                {
                    Id = trip.Id,
                    Destination = trip.Destination,
                    StartDate = trip.StartDate,
                    EndDate = trip.EndDate,
                    TotalItems = trip.TripItems.Count,
                    PackedItems = trip.TripItems.Count(ti => ti.IsPacked)
                })
                .ToListAsync();

            return Ok(trips);
        }

        /// <summary>
        /// GET /api/trips/{id} - Get trip details with items IN the trip
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TripDetailsReadDto>> GetTrip(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .Where(t => t.Id == id && t.MasterUserId == user.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (trip == null)
                return NotFound();

            var tripItems = await _context.TripItems
                .Where(ti => ti.TripId == id)
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

            return new TripDetailsReadDto
            {
                Id = trip.Id,
                Destination = trip.Destination,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                Items = tripItems
            };
        }

        /// <summary>
        /// POST /api/trips - Create a new trip with selected categories
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TripSummaryReadDto>> CreateTrip(TripCreateDto tripDto)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = tripDto.ToTrip();
            trip.MasterUserId = user.Id;
            _context.Trips.Add(trip);

            // Add all items from selected categories to the trip
            if (tripDto.CategoryIds.Any())
            {
                var itemsInCategories = await _context.Items
                    .Where(item => tripDto.CategoryIds.Contains(item.CategoryId) && item.MasterUserId == user.Id)
                    .Select(item => item.Id)
                    .ToListAsync();

                foreach (var itemId in itemsInCategories)
                {
                    _context.TripItems.Add(new Shared.Models.TripItem
                    {
                        Trip = trip,
                        ItemId = itemId,
                        IsPacked = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            var summary = new TripSummaryReadDto
            {
                Id = trip.Id,
                Destination = trip.Destination,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                TotalItems = trip.TripItems.Count,
                PackedItems = 0
            };

            return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, summary);
        }

        /// <summary>
        /// PUT /api/trips/{id} - Update trip metadata (destination, dates)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(int id, TripEditDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .Where(t => t.Id == id && t.MasterUserId == user.Id)
                .FirstOrDefaultAsync();

            if (trip == null)
                return NotFound();

            trip.UpdateFromDto(dto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Trips.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// DELETE /api/trips/{id} - Delete a trip (TripItems cascade automatically)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == id && t.MasterUserId == user.Id);

            if (trip == null)
                return NotFound();

            // TripItems will cascade delete automatically
            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

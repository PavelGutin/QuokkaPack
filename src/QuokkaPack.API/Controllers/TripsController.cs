using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Data.Models;
using QuokkaPack.Shared.DTOs.Trip;
using QuokkaPack.Shared.Mappings;

namespace QuokkaPack.API.Controllers
{
    [Authorize]
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trip>>> GetTrips()
        {
            //TODO: return a DTO
            return await _context.Trips.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TripReadDto>> GetTrip(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .Include(t => t.Categories)
                .FirstOrDefaultAsync(t => t.Id == id && t.MasterUserId == user.Id);

            if (trip == null)
                return NotFound();

            return trip.ToReadDto();
        }

        [HttpPost]
        public async Task<ActionResult<TripReadDto>> CreateTrip(TripCreateDto tripDto)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            // Create the base trip
            var trip = tripDto.ToTrip(); // this should exclude category binding
            trip.MasterUserId = user.Id;

            // Fetch categories that match the provided IDs and belong to the user
            var categories = await _context.Categories
                .Where(c => tripDto.CategoryIds.Contains(c.Id) && c.MasterUserId == user.Id)
                .ToListAsync();

            trip.Categories = categories;

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            try
            {
                return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, trip.ToReadDto());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating CreatedAtAction: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(int id, TripEditDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in body.");

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
                return NotFound();

            //TODO: Use automapper or an extension method to map DTO to entity
            trip.Destination = dto.Destination;
            trip.StartDate = dto.StartDate;
            trip.EndDate = dto.EndDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Trips.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
            {
                return NotFound();
            }

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

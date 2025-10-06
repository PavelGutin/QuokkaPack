using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;


namespace QuokkaPack.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/Trips/{tripId}/Categories")]
    public class TripCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserResolver _userResolver;

        public TripCategoriesController(AppDbContext context, IUserResolver userResolver)
        {
            _context = context;
            _userResolver = userResolver;
        }

        // POST: /api/trips/{tripId}/categories
        [HttpPost]
        public async Task<IActionResult> AddCategoryToTrip(int tripId, [FromBody] int categoryId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.MasterUserId == user.Id);

            var tripItems = await _context.Items
                .Where(item => item.CategoryId == categoryId)
                .Select(item => new TripItem() { ItemId = item.Id, TripId = tripId, IsPacked = false })
                .ToListAsync();

            _context.TripItems.AddRange(tripItems);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: /api/trips/{tripId}/categories/batch
        [HttpPost("batch")]
        public async Task<IActionResult> AddCategoriesToTrip(int tripId, [FromBody] List<int> categoryIds)
        {
            foreach (var categoryId in categoryIds)
            {
                await AddCategoryToTrip(tripId, categoryId);
            }
            return NoContent();
        }

        // DELETE: /api/trips/{tripId}/categories/{categoryId}
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> RemoveCategoryFromTrip(int tripId, int categoryId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);


            var trip = await _context.Trips
                .Include(t => t.TripItems)
                    .ThenInclude(ti => ti.Item)
                        .ThenInclude(i => i.Category)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.MasterUserId == user.Id);

            if (trip == null) return NotFound();

            var toRemove = trip.TripItems
                .Where(ti => ti.Item.Category.Id == categoryId)
                .ToList();

            _context.TripItems.RemoveRange(toRemove);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: /api/trips/{tripId}/categories/batch
        [HttpDelete("batch")]
        public async Task<IActionResult> RemoveCategoriesFromTrip(int tripId, [FromBody] List<int> categoryIds)
        {
            foreach (var categoryId in categoryIds)
            {
                await RemoveCategoryFromTrip(tripId, categoryId);
            }
            return NoContent();
        }

        // PUT: /api/trips/{tripId}/categories/{categoryId}/reset
        [HttpPut("{categoryId}/reset")]
        public async Task<IActionResult> ResetCategoryInTrip(int tripId, int categoryId)
        {
            throw new NotImplementedException();
            //var user = await _userResolver.GetOrCreateAsync(User);

            //var trip = await _context.Trips
            //    .Include(t => t.TripItems)
            //        .ThenInclude(ti => ti.Item)
            //            .ThenInclude(i => i.Category)
            //    .FirstOrDefaultAsync(t => t.Id == tripId && t.MasterUserId == user.Id);

            //if (trip == null) return NotFound();

            //var existingItemIds = trip.TripItems.Select(ti => ti.Item.Id).ToHashSet();

            //var categoryItems = await _context.Items
            //    .Include(i => i.Category)
            //    .Where(i => i.Category.Id == categoryId)
            //    .ToListAsync();

            //foreach (var item in categoryItems)
            //{
            //    if (!existingItemIds.Contains(item.Id))
            //    {
            //        trip.TripItems.Add(new TripItem { Item = item, IsPacked = false });
            //    }
            //}

            //await _context.SaveChangesAsync();
            //return NoContent();
        }
    }
}

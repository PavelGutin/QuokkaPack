using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Shared.DTOs.Category;
using QuokkaPack.Shared.Mappings;

namespace QuokkaPack.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUserResolver _userResolver;
        private readonly AppDbContext _context;

        public CategoriesController(IUserResolver userResolver, AppDbContext context)
        {
            _userResolver = userResolver;
            _context = context;
        }

        /// <summary>
        /// GET /api/categories - Get all categories for user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetCategories([FromQuery] bool includeArchived = false)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var query = _context.Categories
                .Where(c => c.MasterUserId == user.Id);

            if (!includeArchived)
            {
                query = query.Where(c => !c.IsArchived);
            }

            var categories = await query
                .AsNoTracking()
                .Select(c => new CategoryReadDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsDefault = c.IsDefault,
                    IsArchived = c.IsArchived
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// GET /api/categories/{id} - Get single category
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryReadDto>> GetCategory(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var category = await _context.Categories
                .Where(c => c.Id == id && c.MasterUserId == user.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            return category.ToReadDto();
        }

        /// <summary>
        /// POST /api/categories - Create new category
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CategoryReadDto>> CreateCategory(CategoryCreateDto dto)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var category = dto.ToCategory();
            category.MasterUserId = user.Id;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category.ToReadDto());
        }

        /// <summary>
        /// PUT /api/categories/{id} - Update category
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryEditDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var user = await _userResolver.GetOrCreateAsync(User);

            var category = await _context.Categories
                .Where(c => c.Id == id && c.MasterUserId == user.Id)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            category.UpdateFromDto(dto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Categories.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// PUT /api/categories/{id}/archive - Archive (soft delete) category
        /// </summary>
        [HttpPut("{id}/archive")]
        public async Task<IActionResult> ArchiveCategory(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var category = await _context.Categories
                .Include(c => c.Items)
                    .ThenInclude(i => i.TripItems)
                        .ThenInclude(ti => ti.Trip)
                .Where(c => c.Id == id && c.MasterUserId == user.Id)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            // Check if any items in this category are used in trips
            var itemsInTrips = category.Items
                .Where(i => i.TripItems.Any())
                .Select(i => new {
                    ItemName = i.Name,
                    Trips = i.TripItems
                        .Select(ti => new { ti.Trip.Id, ti.Trip.Destination })
                        .Distinct()
                        .ToList()
                })
                .ToList();

            if (itemsInTrips.Any())
            {
                var itemList = string.Join(", ", itemsInTrips.Select(x => x.ItemName));
                var allTrips = itemsInTrips
                    .SelectMany(x => x.Trips)
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                var tripList = string.Join(", ", allTrips.Select(t => t.Destination));

                return BadRequest(new {
                    message = $"Cannot archive category. These items are in use: {itemList}. Remove them from trips first: {tripList}",
                    items = itemsInTrips.Select(x => x.ItemName).ToList(),
                    trips = allTrips
                });
            }

            category.IsArchived = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// PUT /api/categories/{id}/restore - Restore archived category
        /// </summary>
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var category = await _context.Categories
                .Where(c => c.Id == id && c.MasterUserId == user.Id)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            category.IsArchived = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// DELETE /api/categories/{id} - Permanently delete category
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var category = await _context.Categories
                .Include(c => c.Items)
                .Where(c => c.Id == id && c.MasterUserId == user.Id)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            // Check if category contains any items
            if (category.Items.Any())
            {
                return BadRequest("Cannot delete category that contains items");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

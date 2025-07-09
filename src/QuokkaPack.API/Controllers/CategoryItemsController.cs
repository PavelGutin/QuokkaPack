using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.Mappings;

namespace QuokkaPack.API.Controllers
{
    [Authorize]
    [ApiController]
    public class CategoryItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserResolver _userResolver;

        public CategoryItemsController(AppDbContext context, IUserResolver userResolver)
        {
            _context = context;
            _userResolver = userResolver;
        }


        // GET: /api/categories/{categoryId}/items
        [HttpGet("api/categories/{categoryId}/items")]
        public async Task<ActionResult<List<ItemReadDto>>> GetItemsForCategory(int categoryId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var category = await _context.Categories
                .Include(c => c.Items)
                .ThenInclude(i => i.Categories)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.MasterUserId == user.Id);

            if (category == null)
                return NotFound();

            var items = category.Items.Select(i => new ItemReadDto
            {
                Id = i.Id,
                Name = i.Name,
                Notes = i.Notes,
                IsEssential = i.IsEssential,
                // include other fields you care about
            }).ToList();

            return Ok(items);
        }


        // POST: /api/categories/5/items/10
        [HttpPost("api/categories/{categoryId}/items/{itemId}")]
        [HttpPost("api/items/{itemId}/categories/{categoryId}")]
        public async Task<IActionResult> AddItemToCategory(int categoryId, int itemId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.MasterUserId == user.Id);
            var item = await _context.Items
                .Include(i => i.Categories)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.MasterUserId == user.Id);

            if (category == null || item == null)
                return NotFound();

            if (!category.Items.Any(i => i.Id == itemId))
                category.Items.Add(item);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                actionName: nameof(ItemsController.GetItem), // assumes a matching GET method exists
                controllerName: "Items",
                routeValues: new { id = item.Id },
                value: item.ToReadDto() // or return a minimal DTO if preferred
            );
        }

        // DELETE: /api/categories/5/items/10
        [HttpDelete("api/categories/{categoryId}/items/{itemId}")]
        [HttpDelete("api/items/{itemId}/categories/{categoryId}")]
        public async Task<IActionResult> RemoveItemFromCategory(int categoryId, int itemId)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.MasterUserId == user.Id);

            if (category == null)
                return NotFound();

            var item = category.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return NotFound();

            category.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

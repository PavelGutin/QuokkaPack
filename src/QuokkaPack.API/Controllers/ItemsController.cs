using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Shared.DTOs.Item;
using QuokkaPack.Shared.Mappings;

namespace QuokkaPack.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IUserResolver _userResolver;
        private readonly AppDbContext _context;

        public ItemsController(IUserResolver userResolver, AppDbContext context)
        {
            _userResolver = userResolver;
            _context = context;
        }

        /// <summary>
        /// GET /api/items - Get entire user catalog (for client-side joins)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemReadDto>>> GetItems()
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var items = await _context.Items
                .Where(item => item.MasterUserId == user.Id)
                .Include(item => item.Category)
                .AsNoTracking()
                .Select(item => new ItemReadDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    CategoryId = item.CategoryId,
                    CategoryName = item.Category.Name
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// GET /api/items/{id} - Get single item
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemReadDto>> GetItem(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var item = await _context.Items
                .Where(i => i.Id == id && i.MasterUserId == user.Id)
                .Include(i => i.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            return item.ToReadDto();
        }

        /// <summary>
        /// POST /api/items - Create new catalog item
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ItemReadDto>> CreateItem([FromBody] ItemCreateDto itemDto)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var item = itemDto.ToItem();
            item.MasterUserId = user.Id;

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            await _context.Entry(item).Reference(i => i.Category).LoadAsync();

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item.ToReadDto());
        }

        /// <summary>
        /// PUT /api/items/{id} - Update catalog item
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, ItemEditDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var user = await _userResolver.GetOrCreateAsync(User);

            var item = await _context.Items
                .Where(i => i.Id == id && i.MasterUserId == user.Id)
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            item.UpdateFromDto(dto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Items.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// DELETE /api/items/{id} - Delete catalog item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var item = await _context.Items
                .Where(i => i.Id == id && i.MasterUserId == user.Id)
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

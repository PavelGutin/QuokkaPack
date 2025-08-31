using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.Mappings;

namespace QuokkaPack.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")] //TODO: Figure out why I need to specify "Bearer" here
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemReadDto>>> GetItems()
        {
            var items = await _context.Items
                .Include(item => item.Category) 
                .AsNoTracking()
                .Select(item => item.ToReadDto())
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemReadDto>> GetItem(int id)
        {
            var item = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return item.ToReadDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemReadDto>> CreateItem([FromBody] ItemCreateDto itemDto)
        {
            var item = itemDto.ToItem();
            var user = await _userResolver.GetOrCreateAsync(User);
            item.MasterUserId = user.Id;
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            await _context.Entry(item).Reference(i => i.Category).LoadAsync();

            try
            {
                return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item.ToReadDto());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating CreatedAtAction: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, ItemEditDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in body.");

            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound();

            //TODO: Use automapper or an extension method to map DTO to entity
            item.Name = dto.Name;
            item.Category = dto.Category;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Items.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

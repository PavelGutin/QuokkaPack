using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using QuokkaPack.Shared.DTOs.CategoryDTOs;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetCategories()
        {
            var categories = await _context.Categories
                //.Include(category => category.Items)
                .ToListAsync();
            return categories.Select(c => c.ToReadDto()).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryReadDto>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return category.ToReadDto();
        }

        [HttpPost]
        public async Task<ActionResult<CategoryReadDto>> CreateCategory(CategoryCreateDto dto)
        {
            var category = dto.ToCategory();
            var user = await _userResolver.GetOrCreateAsync(User);
            category.MasterUserId = user.Id;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            try
            {
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category.ToReadDto());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating CreatedAtAction: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryEditDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in body.");

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            // TODO: Replace with mapper or extension
            category.Name = dto.Name;
            category.IsDefault = dto.IsDefault;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

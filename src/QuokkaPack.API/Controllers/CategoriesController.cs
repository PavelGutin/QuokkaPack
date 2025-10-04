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
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetCategories()
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var categories = await _context.Categories
                .Where(c => c.MasterUserId == user.Id)
                .AsNoTracking()
                .Select(c => new CategoryReadDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsDefault = c.IsDefault
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
        /// DELETE /api/categories/{id} - Delete category
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var user = await _userResolver.GetOrCreateAsync(User);

            var category = await _context.Categories
                .Where(c => c.Id == id && c.MasterUserId == user.Id)
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

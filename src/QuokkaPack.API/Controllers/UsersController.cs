using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuokkaPack.API.Services;
using QuokkaPack.Data;

namespace QuokkaPack.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IUserResolver _resolver;

        public UsersController(AppDbContext db, IUserResolver resolver)
        {
            _db = db;
            _resolver = resolver;
        }

        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize()
        {
            var user = await _resolver.GetOrCreateAsync(User);
            return Ok();
        }
    }
}

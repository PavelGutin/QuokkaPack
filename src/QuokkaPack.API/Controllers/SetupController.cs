using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.API.Controllers;

[ApiController]
[Route("api/setup")]
public class SetupController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SetupController(AppDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync();
            var hasUsers = _userManager.Users.Any();

            return Ok(new
            {
                databaseReady = canConnect,
                hasUsers
            });
        }
        catch
        {
            return Ok(new
            {
                databaseReady = false,
                hasUsers = false
            });
        }
    }

    public record SetupRequest(string Username, string Password);

    [HttpPost("init")]
    public async Task<IActionResult> InitializeDatabase([FromBody] SetupRequest request)
    {
        await _db.Database.MigrateAsync();

        if (_userManager.Users.Any())
            return BadRequest("Setup already completed.");

        var user = new IdentityUser { UserName = request.Username, Email = request.Username };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        _db.MasterUsers.Add(new MasterUser { IdentityUserId = user.Id });
        await _db.SaveChangesAsync();

        return Ok("Setup completed.");
    }
}

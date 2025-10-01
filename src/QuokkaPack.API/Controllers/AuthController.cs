using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuokkaPack.Data;
using QuokkaPack.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuokkaPack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel request)
    {
        var user = await _userManager.FindByNameAsync(request.Email);
        if (user == null)
            return Unauthorized("Invalid username or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid username or password.");

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { Errors = new[] { new { Description = "Passwords do not match." } } });

        var identityUser = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(identityUser, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Create the MasterUser
        var masterUser = new MasterUser
        {
            IdentityUserId = identityUser.Id,
            Logins = [
                new AppUserLogin
                {
                    Provider = "local",
                    ProviderUserId = identityUser.Id,
                    Email = identityUser.Email,
                }
            ]
        };
        _context.MasterUsers.Add(masterUser);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(identityUser);
        return Ok(new { token });
    }

    private string GenerateJwtToken(IdentityUser user)
    {

        // Look up the MasterUser linked to this IdentityUser
        var masterUserId = _context.MasterUsers
            .Where(mu => mu.IdentityUserId == user.Id)
            .Select(mu => mu.Id)
            .FirstOrDefault();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("master_user_id", masterUserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!))
        {
            KeyId = "quokka-secret" // <- Arbitrary but consistent string
        };
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        token.Header["kid"] = key.KeyId;

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    //public class LoginRequest
    //{
    //    public string Email { get; set; } = string.Empty;
    //    public string Password { get; set; } = string.Empty;
    //}

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

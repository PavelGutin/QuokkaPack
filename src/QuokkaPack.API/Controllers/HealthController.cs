using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.Data;

namespace QuokkaPack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Detailed health check including database connectivity
    /// </summary>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailed()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            checks = new Dictionary<string, object>()
        };

        // Check database connectivity
        try
        {
            await _context.Database.CanConnectAsync();
            health.checks["database"] = new { status = "healthy" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            health.checks["database"] = new { status = "unhealthy", error = ex.Message };
            return StatusCode(503, new
            {
                status = "unhealthy",
                timestamp = health.timestamp,
                version = health.version,
                checks = health.checks
            });
        }

        return Ok(health);
    }
}

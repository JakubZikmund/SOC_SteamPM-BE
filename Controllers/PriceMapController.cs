using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SOC_SteamPM_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceMapController : ControllerBase
    {
        private readonly ILogger<PriceMapController> _logger;

        public PriceMapController(ILogger<PriceMapController> logger)
        {
            _logger = logger;
        }

        // TBD
        [HttpGet("game/{appId:int}")]
        public IActionResult GetGameById(int appId)
        {
            try
            {
                return Ok("This endpoint is not yet implemented.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game by ID {AppId}", appId);
                return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
            }
        }
        
    }
}

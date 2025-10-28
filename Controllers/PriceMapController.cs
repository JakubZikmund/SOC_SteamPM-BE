using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Services.PriceMap;
using SOC_SteamPM_BE.Services.Steam;

namespace SOC_SteamPM_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceMapController : ControllerBase
    {
        private readonly IPriceMapService _priceMapService;
        private readonly ILogger<PriceMapController> _logger;

        public PriceMapController(ILogger<PriceMapController> logger, IPriceMapService priceMapService)
        {
            _priceMapService = priceMapService;
            _logger = logger;
        }

        [HttpGet("game/{appId:int}")]
        public async Task<IActionResult> GetGameById(int appId, [FromQuery] string currency = "EUR")
        {
            try
            {
                var data = await _priceMapService.GetGameInfoAndPrices(appId, currency);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game by ID {AppId}", appId);
                return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
            }
        }
        
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Services.Steam;

namespace SOC_SteamPM_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceMapController : ControllerBase
    {
        private readonly ILogger<PriceMapController> _logger;
        private readonly ISteamApiService _steamApi;

        public PriceMapController(ILogger<PriceMapController> logger, ISteamApiService steamApi)
        {
            _logger = logger;
            _steamApi = steamApi;
        }

        [HttpGet("game/{appId:int}")]
        public async Task<IActionResult> GetGameById(int appId)
        {
            try
            {
                var data = await _steamApi.FetchGameById(appId, "cz");
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

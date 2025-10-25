using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Services;
using SOC_SteamPM_BE.Services.Engine;
using SOC_SteamPM_BE.Services.GameSearch;

namespace SOC_SteamPM_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EngineController : ControllerBase
    {
        private readonly IEngineDataService _engineDataService;
        private readonly IGameSearchRefreshFromApiService _gameSearchRefresh;
        private readonly ILogger<EngineController> _logger;

        public EngineController(IEngineDataService engineDataService, IGameSearchRefreshFromApiService gameSearchRefresh ,ILogger<EngineController> logger)
        {
            _engineDataService = engineDataService;
            _gameSearchRefresh = gameSearchRefresh;
            _logger = logger;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var state = _engineDataService.GetCurrentState();
        
            return Ok(new
            {
                status = state.Status.ToString().ToLower(),
                lastUpdated = state.SteamGamesDictLastUpdated,
                gameCount = state.SteamGamesDict?.Count ?? 0,
                errorMessage = state.ErrorMessage,
                updateAttempts = state.SteamGamesDictUpdateAttempts
            });
        }
        
        [HttpGet("refresh")]
        public async Task<IActionResult> ForceRefresh()
        {
            try
            {
                _logger.LogInformation("Manual refresh requested");
                var success = await _gameSearchRefresh.RefreshFromApiAsync();
                
                if (success)
                {
                    return Ok(new { message = "Refresh completed successfully" });
                }

                return StatusCode(500, new { message = "Refresh failed after all attempts" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual refresh");
                return StatusCode(500, new { message = "Refresh failed due to unexpected error" });
            }
        }
    }
}

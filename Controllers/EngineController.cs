using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Services;

namespace SOC_SteamPM_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EngineController : ControllerBase
    {
        private readonly IGameSearchService _gameSearchService;
        private readonly IGameCacheService _gameCacheService;
        private readonly ILogger<EngineController> _logger;

        public EngineController(IGameSearchService gameSearchService, IGameCacheService gameCacheService, ILogger<EngineController> logger)
        {
            _gameSearchService = gameSearchService;
            _gameCacheService = gameCacheService;
            _logger = logger;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var state = _gameSearchService.GetCurrentState();
        
            return Ok(new
            {
                status = state.Status.ToString().ToLower(),
                lastUpdated = state.GameSearchLastUpdated,
                gameCount = state.GameSearchData?.Applist?.Apps?.Count ?? 0,
                errorMessage = state.ErrorMessage,
                updateAttempts = state.GameSearchUpdateAttempts
            });
        }
        
        [HttpGet("refresh")]
        public async Task<IActionResult> ForceRefresh()
        {
            try
            {
                _logger.LogInformation("Manual refresh requested");
                var success = await _gameSearchService.ForceRefreshAsync();
            
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

        [HttpGet("clearCache")]
        public async Task<IActionResult> ClearCache()
        {
            try
            {
                _gameCacheService.ClearCache();
                _logger.LogInformation("Cache cleared");
                return Ok(new { message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache clear");
                return StatusCode(500, new { message = "Cache clear failed due to unexpected error" });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SOC_SteamPM_BE.Services;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameSearchController : ControllerBase
{
    private readonly IGameDataService _gameDataService;
    private readonly ILogger<GameSearchController> _logger;

    public GameSearchController(IGameDataService gameDataService, ILogger<GameSearchController> logger)
    {
        _gameDataService = gameDataService;
        _logger = logger;
    }

    [HttpGet("games")]
    public async Task<IActionResult> GetGames()
    {
        try
        {
            var games = await _gameDataService.GetGamesAsync();
            return Ok(games);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("GetGames failed: {Message}", ex.Message);
            
            // Return specific error based on current state
            var state = _gameDataService.GetCurrentState();
            
            if (state.Status == DataStatus.Updating)
            {
                return StatusCode(503, new { 
                    error = "Service Updating", 
                    message = "Game data is currently being updated. Please try again later.",
                    status = "updating"
                });
            }
            
            if (state.Status == DataStatus.Loading)
            {
                return StatusCode(503, new { 
                    error = "Service Loading", 
                    message = "Game data is still loading. Please try again later.",
                    status = "loading"
                });
            }
            
            return StatusCode(500, new { 
                error = "Service Error", 
                message = ex.Message,
                status = "error"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting games");
            return StatusCode(500, new { 
                error = "Internal Server Error", 
                message = "An unexpected error occurred" 
            });
        }
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var state = _gameDataService.GetCurrentState();
        
        return Ok(new
        {
            status = state.Status.ToString().ToLower(),
            lastUpdated = state.LastUpdated,
            gameCount = state.Data?.Applist?.Apps?.Count ?? 0,
            errorMessage = state.ErrorMessage,
            updateAttempts = state.UpdateAttempts
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> ForceRefresh()
    {
        try
        {
            _logger.LogInformation("Manual refresh requested");
            var success = await _gameDataService.ForceRefreshAsync();
            
            if (success)
            {
                return Ok(new { message = "Refresh completed successfully" });
            }
            else
            {
                return StatusCode(500, new { message = "Refresh failed after all attempts" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual refresh");
            return StatusCode(500, new { message = "Refresh failed due to unexpected error" });
        }
    }
}

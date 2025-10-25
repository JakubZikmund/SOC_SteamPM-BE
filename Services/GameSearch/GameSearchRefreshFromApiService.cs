using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Services.Steam;
using SOC_SteamPM_BE.Utils;

namespace SOC_SteamPM_BE.Services.GameSearch;

public interface IGameSearchRefreshFromApiService
{
    Task<bool> RefreshFromApiAsync();
}

public class GameSearchRefreshFromApiService : IGameSearchRefreshFromApiService
{
    private readonly ISteamApiService _steamApi;
    private readonly IEngineDataManager _dataManager;
    private readonly ILogger<GameSearchRefreshFromApiService> _logger;
    
    private const int MaxAttempts = 3;
    private const int BaseDelaySeconds = 5;

    public GameSearchRefreshFromApiService(
        ISteamApiService steamApi,
        IEngineDataManager dataManager,
        ILogger<GameSearchRefreshFromApiService> logger)
    {
        _steamApi = steamApi;
        _dataManager = dataManager;
        _logger = logger;
    }

    public async Task<bool> RefreshFromApiAsync()
    {
        _logger.LogInformation("Starting game data refresh from Steam API");
        
        await _dataManager.SetUpdatingStateAsync();
        await _dataManager.ResetUpdateAttemptsAsync();

        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation("Refresh attempt {Attempt}/{MaxAttempts}", attempt, MaxAttempts);
                
                var newData = await _steamApi.FetchAllGames();

                var  games = DataFactory.DictionaryFromSteamGames(newData);
                
                await _dataManager.UpdateSteamGamesDictAsync(games);
                
                _logger.LogInformation("Refresh completed successfully on attempt {Attempt}", attempt);
                return true;
            }
            catch (Exception ex)
            {
                await _dataManager.IncrementUpdateAttemptsAsync();
                _logger.LogError(ex, "Refresh attempt {Attempt}/{MaxAttempts} failed: {Error}", 
                    attempt, MaxAttempts, ex.Message);

                if (attempt == MaxAttempts)
                {
                    var errorMessage = $"Failed to refresh after {MaxAttempts} attempts. Last error: {ex.Message}";
                    await _dataManager.SetSteamGamesDictUpdateErrorAsync(errorMessage);
                    _logger.LogError("All refresh attempts failed. Keeping existing data if available.");
                    return false;
                }

                // Exponential backoff: 10s, 20s, 40s
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt) * BaseDelaySeconds);
                _logger.LogInformation("Waiting {Delay} seconds before next attempt...", delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }

        return false;
    }
}


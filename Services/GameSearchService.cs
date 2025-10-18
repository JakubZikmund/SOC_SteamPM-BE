using SOC_SteamPM_BE.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace SOC_SteamPM_BE.Services;

public interface IGameSearchService
{
    // Task<SteamGameResponse> GetGamesAsync();
    Task<bool> ForceRefreshAsync();
    EngineDataState GetCurrentState();
}

public class GameSearchService : IGameSearchService
{
    private readonly ISteamApiService _steamApi;
    private readonly DataStorageSettings _dataSettings;
    private readonly IEngineDataManager _dataManager;
    private readonly ILogger<GameSearchService> _logger;

    public GameSearchService(
        ISteamApiService steamApi, 
        IOptions<DataStorageSettings> dataSettings,
        IEngineDataManager dataManager,
        ILogger<GameSearchService> logger)
    {
        _steamApi = steamApi;
        _dataSettings = dataSettings.Value;
        _dataManager = dataManager;
        _logger = logger;
    }

    // public async Task<SteamGameResponse> GetGamesAsync()
    // {
    //     var currentData = _dataManager.GetCurrentData();
    //     var currentStatus = _dataManager.GetCurrentStatus();
    //
    //     // If we're updating, return error
    //     if (currentStatus == EngineStatus.Updating)
    //     {
    //         throw new InvalidOperationException("Game data is currently being updated. Please try again later.");
    //     }
    //
    //     // If we have data, return it
    //     if (currentData != null)
    //     {
    //         return currentData;
    //     }
    //
    //     // No data available and not updating, this should not happen after startup
    //     if (currentStatus == EngineStatus.Error)
    //     {
    //         throw new InvalidOperationException("Game data is not available due to initialization errors.");
    //     }
    //
    //     throw new InvalidOperationException("Game data is still loading. Please try again later.");
    // }

    public async Task<bool> ForceRefreshAsync()
    {
        const int maxAttempts = 3;
        
        await _dataManager.SetUpdatingStateAsync();
        await _dataManager.ResetUpdateAttemptsAsync();

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation("Starting refresh attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);
                
                // Fetch new data from Steam API
                var newData = await _steamApi.FetchAllGames();
                
                // Update manager with new data
                await _dataManager.UpdateDataAsync(newData);
                
                _logger.LogInformation("Refresh completed successfully on attempt {Attempt}", attempt);
                return true;
            }
            catch (Exception ex)
            {
                await _dataManager.IncrementUpdateAttemptsAsync();
                _logger.LogError(ex, "Refresh attempt {Attempt}/{MaxAttempts} failed: {Error}", 
                    attempt, maxAttempts, ex.Message);

                if (attempt == maxAttempts)
                {
                    // All attempts failed
                    await _dataManager.SetErrorStateAsync($"Failed to refresh after {maxAttempts} attempts. Last error: {ex.Message}");
                    _logger.LogError("All refresh attempts failed. Keeping existing data.");
                    return false;
                }

                // Wait before next attempt
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt) * 5); // 10s, 20s, 40s
                _logger.LogInformation("Waiting {Delay} seconds before next attempt...", delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }

        return false;
    }

    public EngineDataState GetCurrentState()
    {
        return _dataManager.GetCurrentState();
    }

    // Vstupní bod po zapnutí aplikace
    public async Task InitializeAsync()
    {
        
        // It will try to load data from an existing file first if we enable it in appsettings.json
        if (_dataSettings.LoadDataFromFileOnStartup)
        {
            var filePath = Path.Combine(_dataSettings.FolderPath, "games.json");

            try
            {
                // Try to load an existing file first
                if (File.Exists(filePath))
                {
                    _logger.LogInformation("Loading existing games file on startup: {FilePath}", filePath);
                    var existingData = await LoadFromFileAsync(filePath);
                    await _dataManager.UpdateDataAsync(existingData);
                    _logger.LogInformation("Existing data loaded successfully");
                    return;
                }

                _logger.LogInformation("No existing file found, downloading fresh data on startup - trying to fetch from Steam API");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load data from existing file");
            }
        }

        // Steam API Fetch - Refresh        
        if (!await ForceRefreshAsync())
        {
            _logger.LogCritical("Failed to fetch data from Steam API. Application will now exit.");
            Environment.Exit(1);
        }

        await Task.CompletedTask;
    }

    private async Task<SteamGameResponse> LoadFromFileAsync(string filePath)
    {
        var jsonContent = await File.ReadAllTextAsync(filePath);
        var gameData = JsonSerializer.Deserialize<SteamGameResponse>(jsonContent);
        return gameData ?? new SteamGameResponse();
    }
}


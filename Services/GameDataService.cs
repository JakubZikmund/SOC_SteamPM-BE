using SOC_SteamPM_BE.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace SOC_SteamPM_BE.Services;

public interface IGameDataService
{
    Task<SteamGameResponse> GetGamesAsync();
    Task<bool> ForceRefreshAsync();
    GameDataState GetCurrentState();
}

public class GameDataService : IGameDataService
{
    private readonly HttpClient _httpClient;
    private readonly SteamApiSettings _steamSettings;
    private readonly DataStorageSettings _dataSettings;
    private readonly IGameDataManager _dataManager;
    private readonly ILogger<GameDataService> _logger;

    public GameDataService(
        HttpClient httpClient,
        IOptions<SteamApiSettings> steamSettings,
        IOptions<DataStorageSettings> dataSettings,
        IGameDataManager dataManager,
        ILogger<GameDataService> logger)
    {
        _httpClient = httpClient;
        _steamSettings = steamSettings.Value;
        _dataSettings = dataSettings.Value;
        _dataManager = dataManager;
        _logger = logger;
    }

    public async Task<SteamGameResponse> GetGamesAsync()
    {
        var currentData = _dataManager.GetCurrentData();
        var currentStatus = _dataManager.GetCurrentStatus();

        // If we're updating, return error
        if (currentStatus == DataStatus.Updating)
        {
            throw new InvalidOperationException("Game data is currently being updated. Please try again later.");
        }

        // If we have data, return it
        if (currentData != null)
        {
            return currentData;
        }

        // No data available and not updating, this should not happen after startup
        if (currentStatus == DataStatus.Error)
        {
            throw new InvalidOperationException("Game data is not available due to initialization errors.");
        }

        throw new InvalidOperationException("Game data is still loading. Please try again later.");
    }

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
                var newData = await FetchFromSteamApiAsync();
                
                // If successful, update file and manager
                var filePath = Path.Combine(_dataSettings.FolderPath, "games.json");
                await UpdateFileAsync(filePath, newData);
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

                // Wait before next attempt (exponential backoff)
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt) * 5); // 10s, 20s, 40s
                _logger.LogInformation("Waiting {Delay} seconds before next attempt...", delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }

        return false;
    }

    public GameDataState GetCurrentState()
    {
        return _dataManager.GetCurrentState();
    }

    public async Task InitializeAsync()
    {
        var filePath = Path.Combine(_dataSettings.FolderPath, "games.json");

        try
        {
            // Try to load existing file first
            if (File.Exists(filePath))
            {
                _logger.LogInformation("Loading existing games file on startup: {FilePath}", filePath);
                var existingData = await LoadFromFileAsync(filePath);
                await _dataManager.UpdateDataAsync(existingData);
                _logger.LogInformation("Existing data loaded successfully");
                return;
            }

            _logger.LogInformation("No existing file found, downloading fresh data on startup");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load existing file, will download fresh data");
        }

        // No existing file or failed to load, download fresh data
        await ForceRefreshAsync();
    }

    private async Task<SteamGameResponse> LoadFromFileAsync(string filePath)
    {
        var jsonContent = await File.ReadAllTextAsync(filePath);
        var gameData = JsonSerializer.Deserialize<SteamGameResponse>(jsonContent);
        return gameData ?? new SteamGameResponse();
    }

    private async Task<SteamGameResponse> FetchFromSteamApiAsync()
    {
        _logger.LogInformation("Calling Steam API: {Url}", _steamSettings.BaseUrl);
        
        var response = await _httpClient.GetAsync(_steamSettings.BaseUrl);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var gameData = JsonSerializer.Deserialize<SteamGameResponse>(jsonContent);
        
        _logger.LogInformation("Successfully fetched {GameCount} games from Steam API", 
            gameData?.Applist?.Apps?.Count ?? 0);

        return gameData ?? new SteamGameResponse();
    }

    private async Task UpdateFileAsync(string filePath, SteamGameResponse gameData)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation("Created directory: {Directory}", directory);
        }

        // Write to temporary file first for atomic operation
        var tempFilePath = filePath + ".tmp";
        var jsonContent = JsonSerializer.Serialize(gameData, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        await File.WriteAllTextAsync(tempFilePath, jsonContent);

        // Delete old file if exists and rename temp file (atomic operation)
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Deleted old games file: {FilePath}", filePath);
        }

        File.Move(tempFilePath, filePath);
        _logger.LogInformation("Successfully updated games file: {FilePath} with {GameCount} games", 
            filePath, gameData?.Applist?.Apps?.Count ?? 0);
    }
}

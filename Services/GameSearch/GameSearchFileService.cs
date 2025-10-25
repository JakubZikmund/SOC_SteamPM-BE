using System.Text.Json;
using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services.GameSearch;

/// <summary>
/// Handles file I/O operations for game search data persistence.
/// </summary>
public interface IGameSearchFileService
{
    Task<SteamGamesResponse?> LoadFromFileAsync(string filePath);
    bool FileExists(string filePath);
    string GetDefaultFilePath();
}

public class GameSearchFileService : IGameSearchFileService
{
    private readonly DataStorageSettings _dataSettings;
    private readonly ILogger<GameSearchFileService> _logger;

    public GameSearchFileService(
        IOptions<DataStorageSettings> dataSettings,
        ILogger<GameSearchFileService> logger)
    {
        _dataSettings = dataSettings.Value;
        _logger = logger;
    }

    public async Task<SteamGamesResponse?> LoadFromFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Loading game data from file: {FilePath}", filePath);
            
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var gameData = JsonSerializer.Deserialize<SteamGamesResponse>(jsonContent);
            
            _logger.LogInformation("Successfully loaded {Count} games from file", 
                gameData?.Applist.Apps.Count ?? 0);
            
            return gameData ?? new SteamGamesResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load game data from file: {FilePath}", filePath);
            return null;
        }
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public string GetDefaultFilePath()
    {
        return Path.Combine(_dataSettings.FolderPath, "games.json");
    }
}


using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Utils;

namespace SOC_SteamPM_BE.Services.GameSearch;

public interface IGameSearchInitializationService
{
    Task InitializeAsync();
}

public class GameSearchInitializationService : IGameSearchInitializationService
{
    private readonly IGameSearchFileService _fileService;
    private readonly IGameSearchRefreshFromApiService _refreshFromApiService;
    private readonly IEngineDataManager _dataManager;
    private readonly DataStorageSettings _dataSettings;
    
    private readonly ILogger<GameSearchInitializationService> _logger;

    public GameSearchInitializationService(
        IGameSearchFileService fileService,
        IGameSearchRefreshFromApiService refreshFromApiService,
        IEngineDataManager dataManager,
        IOptions<DataStorageSettings> dataSettings,
        ILogger<GameSearchInitializationService> logger)
    {
        _fileService = fileService;
        _refreshFromApiService = refreshFromApiService;
        _dataManager = dataManager;
        _dataSettings = dataSettings.Value;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting game search data initialization...");

        // Try to load from file if enabled
        if (_dataSettings.LoadDataFromFileOnStartup)
        {
            var filePath = _fileService.GetDefaultFilePath();

            if (_fileService.FileExists(filePath))
            {
                try
                {
                    
                    var fileData = await _fileService.LoadFromFileAsync(filePath);
                    
                    // build name based dictionary
                    var gamesByNameLowercase = DataFactory.DictionaryFromSteamGames(fileData);
                
                    await _dataManager.UpdateSteamGamesDictAsync(gamesByNameLowercase);
                    _logger.LogInformation("Initialization completed successfully using cached file");
                    return;    
                    
                } 
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to load game data from file.");
                }
                
            }
            _logger.LogInformation("No cached file found at: {FilePath}", filePath);
        }
        else
        {
            _logger.LogInformation("File loading disabled in settings, fetching from Steam API");
        }

        // Strategy 2: Fetch from Steam API
        _logger.LogInformation("Fetching fresh data from Steam API...");
        
        var success = await _refreshFromApiService.RefreshFromApiAsync();
        
        if (!success)
        {
            _logger.LogCritical("Failed to initialize game search data. Application cannot continue.");
            Environment.Exit(1);
        }

        _logger.LogInformation("Initialization completed successfully using Steam API");
    }
}


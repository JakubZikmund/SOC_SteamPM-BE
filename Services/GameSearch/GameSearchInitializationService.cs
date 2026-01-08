namespace SOC_SteamPM_BE.Services.GameSearch;

public interface IGameSearchInitializationService
{
    Task InitializeAsync();
}

public class GameSearchInitializationService : IGameSearchInitializationService
{
    private readonly IGameSearchRefreshFromApiService _refreshFromApiService;
    
    private readonly ILogger<GameSearchInitializationService> _logger;

    public GameSearchInitializationService(
        IGameSearchRefreshFromApiService refreshFromApiService,
        ILogger<GameSearchInitializationService> logger)
    {
        _refreshFromApiService = refreshFromApiService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting game search data initialization...");
        
        var success = await _refreshFromApiService.RefreshFromApiAsync();
        
        if (!success)
        {
            _logger.LogCritical("Failed to initialize game search data. Application cannot continue.");
            Environment.Exit(1);
        }

        _logger.LogInformation("Initialization completed successfully using Steam API");
    }
}


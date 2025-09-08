using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services;

public interface IEngineDataManager
{
    EngineDataState GetCurrentState();
    // SteamGameResponse? GetCurrentData();
    // EngineStatus GetCurrentStatus();
    Task SetUpdatingStateAsync();
    Task UpdateDataAsync(SteamGameResponse newData);
    Task SetErrorStateAsync(string errorMessage);
    Task ResetUpdateAttemptsAsync();
    Task IncrementUpdateAttemptsAsync();
    // int GetUpdateAttempts();
}

public class EngineDataManager : IEngineDataManager
{
    private readonly object _lock = new();
    private EngineDataState _currentState = new();
    private readonly ILogger<EngineDataManager> _logger;
    private readonly IGameCacheService _gameCache;

    public EngineDataManager(ILogger<EngineDataManager> logger, IGameCacheService gameCache)
    {
        _logger = logger;
        _gameCache = gameCache;
    }

    public EngineDataState GetCurrentState()
    {
        lock (_lock)
        {
            return new EngineDataState
            {
                Status = _currentState.Status,
                GameSearchData = _currentState.GameSearchData,
                GameSearchLastUpdated = _currentState.GameSearchLastUpdated,
                ErrorMessage = _currentState.ErrorMessage,
                GameSearchUpdateAttempts = _currentState.GameSearchUpdateAttempts
            };
        }
    }

    // public SteamGameResponse? GetCurrentData()
    // {
    //     lock (_lock)
    //     {
    //         return _currentState.GameSearchData;
    //     }
    // }

    // public EngineStatus GetCurrentStatus()
    // {
    //     lock (_lock)
    //     {
    //         return _currentState.Status;
    //     }
    // }

    public Task SetUpdatingStateAsync()
    {
        lock (_lock)
        {
            _currentState.Status = EngineStatus.Updating;
            _currentState.ErrorMessage = null;
            _logger.LogInformation("Game data status set to: Updating");
        }
        return Task.CompletedTask;
    }

    public Task UpdateDataAsync(SteamGameResponse newData)
    {
        lock (_lock)
        {
            _currentState.GameSearchData = newData;
            _currentState.Status = EngineStatus.Ready;
            _currentState.GameSearchLastUpdated = DateTime.Now;
            _currentState.ErrorMessage = null;
            _currentState.GameSearchUpdateAttempts = 0;
            
            // Load data into cache for fast lookups
            _gameCache.LoadGames(newData);
            
            _logger.LogInformation("Game data updated successfully. Status: Ready, Games count: {Count}", 
                newData?.Applist?.Apps?.Count ?? 0);
        }
        return Task.CompletedTask;
    }

    public Task SetErrorStateAsync(string errorMessage)
    {
        lock (_lock)
        {
            // Only set error state if we don't have any data yet
            if (_currentState.GameSearchData == null)
            {
                _currentState.Status = EngineStatus.Error;
            }
            else
            {
                // We have old data, keep it and set status back to Ready
                _currentState.Status = EngineStatus.Ready;
            }
            
            _currentState.ErrorMessage = errorMessage;
            _logger.LogError("Game data error: {ErrorMessage}. Status: {Status}", 
                errorMessage, _currentState.Status);
        }
        return Task.CompletedTask;
    }

    public Task ResetUpdateAttemptsAsync()
    {
        lock (_lock)
        {
            _currentState.GameSearchUpdateAttempts = 0;
        }
        return Task.CompletedTask;
    }

    public Task IncrementUpdateAttemptsAsync()
    {
        lock (_lock)
        {
            _currentState.GameSearchUpdateAttempts++;
            _logger.LogWarning("Update attempt #{Attempt} failed", _currentState.GameSearchUpdateAttempts);
        }
        return Task.CompletedTask;
    }

    // public int GetUpdateAttempts()
    // {
    //     lock (_lock)
    //     {
    //         return _currentState.GameSearchUpdateAttempts;
    //     }
    // }
}


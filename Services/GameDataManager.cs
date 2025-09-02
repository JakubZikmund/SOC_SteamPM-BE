using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services;

public interface IGameDataManager
{
    GameDataState GetCurrentState();
    SteamGameResponse? GetCurrentData();
    DataStatus GetCurrentStatus();
    Task SetUpdatingStateAsync();
    Task UpdateDataAsync(SteamGameResponse newData);
    Task SetErrorStateAsync(string errorMessage);
    Task ResetUpdateAttemptsAsync();
    Task IncrementUpdateAttemptsAsync();
    int GetUpdateAttempts();
}

public class GameDataManager : IGameDataManager
{
    private readonly object _lock = new();
    private GameDataState _currentState = new();
    private readonly ILogger<GameDataManager> _logger;

    public GameDataManager(ILogger<GameDataManager> logger)
    {
        _logger = logger;
    }

    public GameDataState GetCurrentState()
    {
        lock (_lock)
        {
            return new GameDataState
            {
                Status = _currentState.Status,
                Data = _currentState.Data,
                LastUpdated = _currentState.LastUpdated,
                ErrorMessage = _currentState.ErrorMessage,
                UpdateAttempts = _currentState.UpdateAttempts
            };
        }
    }

    public SteamGameResponse? GetCurrentData()
    {
        lock (_lock)
        {
            return _currentState.Data;
        }
    }

    public DataStatus GetCurrentStatus()
    {
        lock (_lock)
        {
            return _currentState.Status;
        }
    }

    public Task SetUpdatingStateAsync()
    {
        lock (_lock)
        {
            _currentState.Status = DataStatus.Updating;
            _currentState.ErrorMessage = null;
            _logger.LogInformation("Game data status set to: Updating");
        }
        return Task.CompletedTask;
    }

    public Task UpdateDataAsync(SteamGameResponse newData)
    {
        lock (_lock)
        {
            _currentState.Data = newData;
            _currentState.Status = DataStatus.Ready;
            _currentState.LastUpdated = DateTime.Now;
            _currentState.ErrorMessage = null;
            _currentState.UpdateAttempts = 0;
            
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
            if (_currentState.Data == null)
            {
                _currentState.Status = DataStatus.Error;
            }
            else
            {
                // We have old data, keep it and set status back to Ready
                _currentState.Status = DataStatus.Ready;
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
            _currentState.UpdateAttempts = 0;
        }
        return Task.CompletedTask;
    }

    public Task IncrementUpdateAttemptsAsync()
    {
        lock (_lock)
        {
            _currentState.UpdateAttempts++;
            _logger.LogWarning("Update attempt #{Attempt} failed", _currentState.UpdateAttempts);
        }
        return Task.CompletedTask;
    }

    public int GetUpdateAttempts()
    {
        lock (_lock)
        {
            return _currentState.UpdateAttempts;
        }
    }
}

using Microsoft.Extensions.Caching.Memory;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Managers;

public interface IEngineDataManager
{
    EngineDataState GetCurrentState();
    Dictionary<string, SearchGameModel> GetSteamGamesDict();
    Task SetUpdatingStateAsync();
    Task UpdateSteamGamesDictAsync(Dictionary<string, SearchGameModel> newData);
    Task SetSteamGamesDictUpdateErrorAsync(string errorMessage);
    Task ResetUpdateAttemptsAsync();
    Task IncrementUpdateAttemptsAsync();
    Task SetEngineStateAsync(EngineStatus status);
    Task InitializeAsync();
    bool TryGetCachedPriceMap(string gameKey, out GameInfo priceMap);
    void SetCachedPriceMap(string gameKey, GameInfo priceMap);
    bool TryGetCachedCurrency(string currKey, out CurrencyModel currData);
    void SetCachedCurrency(string currKey, CurrencyModel currData);
    void SetCachedWishlistGame(string wishKey, WishlistGame wishData);
    bool TryGetCachedWishlistGame(string wishKey, out WishlistGame wishData);
}

public class EngineDataManager : IEngineDataManager
{
    private readonly IMemoryCache _cache;
    private readonly object _attemptsLock = new object();

    private const string STATUS_KEY = "Status";
    private const string STEAM_GAMES_KEY = "SteamGamesDict";
    private const string STEAM_GAMES_LAST_UPDATED_KEY = "SteamGamesDictLastUpdated";
    private const string STEAM_GAMES_UPDATE_ATTEMPTS = "SteamGamesDictUpdateAttempts";
    
    private readonly ILogger<EngineDataManager> _logger;

    public EngineDataManager(ILogger<EngineDataManager> logger, IMemoryCache cache)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task InitializeAsync()
    {
        _cache.Set(STATUS_KEY, EngineStatus.Loading);
        _cache.Set(STEAM_GAMES_KEY, new Dictionary<string, SearchGameModel>());
        _cache.Set(STEAM_GAMES_LAST_UPDATED_KEY, DateTime.MinValue);
        _cache.Set(STEAM_GAMES_UPDATE_ATTEMPTS, 0);
        
        _logger.LogInformation("Data manager initialized successfully.");
        
        return Task.CompletedTask;
    }
    
    public EngineDataState GetCurrentState()
    {
        if (_cache.TryGetValue(STATUS_KEY, out EngineStatus status)
            && _cache.TryGetValue(STEAM_GAMES_KEY, out Dictionary<string, SearchGameModel>? steamGamesDict)
            && _cache.TryGetValue(STEAM_GAMES_LAST_UPDATED_KEY, out DateTime steamGamesDictLastUpdated)
            && _cache.TryGetValue(STEAM_GAMES_UPDATE_ATTEMPTS, out int steamGamesDictUpdateAttempts))
        {
            return new EngineDataState
            {
                Status = status,
                SteamGamesDict = steamGamesDict,
                SteamGamesDictLastUpdated = steamGamesDictLastUpdated,
                SteamGamesDictUpdateAttempts = steamGamesDictUpdateAttempts
            };
        }
        throw new Exception("Engine data state is missing some values");
    }

    public Task SetUpdatingStateAsync()
    {
        _cache.Set(STATUS_KEY, EngineStatus.Updating);
        _logger.LogInformation("Game data status set to: Updating");
        return Task.CompletedTask;
    }

    public Task UpdateSteamGamesDictAsync(Dictionary<string, SearchGameModel> newData)
    {
        _cache.Set(STEAM_GAMES_KEY, newData);
        _cache.Set(STATUS_KEY, EngineStatus.Ready);
        _cache.Set(STEAM_GAMES_LAST_UPDATED_KEY, DateTime.Now);
        _cache.Set(STEAM_GAMES_UPDATE_ATTEMPTS, 0);
        
        _logger.LogInformation("Game data updated successfully. Status: Ready, Games count: {Count}", 
            newData.Count);
        
        return Task.CompletedTask;
    }

    public Task SetSteamGamesDictUpdateErrorAsync(string errorMessage)
    {
        var currentState = GetCurrentState();

        _cache.Set(STATUS_KEY, currentState.SteamGamesDict == null ? EngineStatus.Error : EngineStatus.Ready);

        _logger.LogError("Steam games list update error: {ErrorMessage}. Status: {Status}", errorMessage, currentState.Status);
        
        return Task.CompletedTask;
    }

    public Task SetEngineStateAsync(EngineStatus status)
    {
        _cache.Set(STATUS_KEY, status);
        _logger.LogInformation("Engine status set to: {Status}", status);
        return Task.CompletedTask;
    }

    public Task ResetUpdateAttemptsAsync()
    {
        _cache.Set(STEAM_GAMES_UPDATE_ATTEMPTS, 0);
        return Task.CompletedTask;
    }

    public Task IncrementUpdateAttemptsAsync()
    {
        lock (_attemptsLock)
        {
            var currentState = GetCurrentState();
            var newAttempts = currentState.SteamGamesDictUpdateAttempts + 1;
            _cache.Set(STEAM_GAMES_UPDATE_ATTEMPTS, newAttempts);
            _logger.LogWarning("Update attempt #{Attempt} failed", newAttempts);  
            return Task.CompletedTask;
        }
    }
    
    public Dictionary<string, SearchGameModel> GetSteamGamesDict()
    {
        if (_cache.TryGetValue(STEAM_GAMES_KEY, out Dictionary<string, SearchGameModel>? games))
        {
            return games ?? throw new Exception("Steam games dictionary is missing");
        }
        throw new Exception("Steam games dictionary is missing");
    }

    public bool TryGetCachedPriceMap(string gameKey, out GameInfo priceMap)
    {
        if (_cache.TryGetValue(gameKey, out priceMap))
        {
            _logger.LogInformation($"Price map for game with key {gameKey} was found in cache");
            return true;
        }
        _logger.LogInformation($"Price map for game with key {gameKey} was not found in cache");
        return false;
    }

    public void SetCachedPriceMap(string gameKey, GameInfo priceMap)
    {
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(24));
        _cache.Set(gameKey, priceMap, cacheOptions);
    }
    
    public bool TryGetCachedCurrency(string currKey, out CurrencyModel currData)
    {
        if (_cache.TryGetValue(currKey, out currData))
        {
            _logger.LogInformation($"Currency with key {currKey} was found in cache");
            return true;
        }
        _logger.LogInformation($"Currency with key {currKey} was not found in cache");
        return false;
    }

    public void SetCachedCurrency(string currKey, CurrencyModel currData)
    {
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(24));
        _cache.Set(currKey, currData, cacheOptions);
    }
    
    public bool TryGetCachedWishlistGame(string wishKey, out WishlistGame wishData)
    {
        if (_cache.TryGetValue(wishKey, out wishData))
        {
            _logger.LogInformation($"Wishlist game with key {wishKey} was found in cache");
            return true;
        }
        _logger.LogInformation($"Wishlist game with key {wishKey} was not found in cache");
        return false;
    }

    public void SetCachedWishlistGame(string wishKey, WishlistGame wishData)
    {
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(24));
        _cache.Set(wishKey, wishData, cacheOptions);
    }
}


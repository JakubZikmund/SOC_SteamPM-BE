using Microsoft.Extensions.Options;
using SOC_SteamPM_BE.Exceptions;
using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Services.Currencies;
using SOC_SteamPM_BE.Services.Steam;
using SOC_SteamPM_BE.Utils;

namespace SOC_SteamPM_BE.Services.PriceMap;

public interface IPriceMapService
{
    Task<GameInfo> GetGameInfoAndPrices(int appId, string currency);
}    

public class PriceMapService : IPriceMapService
{
    private readonly IEngineDataManager _dataManager;
    private readonly ISteamApiService _steamApi;
    private readonly ICurrencyService _currencyService;
    private readonly IOptions<SteamApiSettings> _steamApiSettings;
    
    private readonly ILogger<PriceMapService> _logger;
    
    public PriceMapService(IEngineDataManager dataManager, ISteamApiService steamApi, ICurrencyService currencyService, IOptions<SteamApiSettings> steamApiSettings, ILogger<PriceMapService> logger)
    {
        _dataManager = dataManager;
        _steamApi = steamApi;
        _currencyService = currencyService;
        _steamApiSettings = steamApiSettings;
        _logger = logger;
    }
    
    
    public async Task<GameInfo> GetGameInfoAndPrices(int appId, string currency = "EUR")
    {
        
        try
        {
            if (!_dataManager.TryGetCachedPriceMap(CacheKeys.Game(appId), out var gameInfo))
            {

                gameInfo = GameObjectBuilder(await _steamApi.FetchGameById(appId, "cz"), await _steamApi.FetchGamePrices(appId, _steamApiSettings.Value.CountryCodes));
            
                // uložení do paměti
                _dataManager.SetCachedPriceMap(CacheKeys.Game(appId), gameInfo);
            }
            
            await _currencyService.ConvertPrices(gameInfo.PriceOverview, currency);
            
            return gameInfo;
        }
        catch (GameNotFoundException)
        {
            throw;
        }
        catch (InvalidCurrencyException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred while fetching game info and prices for AppId {AppId}", appId);
            throw new Exception("An error occurred while fetching game info and prices.", e);
        }
    }

    GameInfo GameObjectBuilder(SteamGameApiResponse gameInfo, List<SteamPrice> steamPrices)
    {
        var game = new GameInfo(gameInfo.Name, gameInfo.AppId, gameInfo.ShortDescription, gameInfo.ReleaseDate.Date, gameInfo.Developers, gameInfo.Publishers, gameInfo.HeaderImage);
        
        game.Categories = gameInfo.Categories.Select(c => c.Description).ToList();
        game.Genres = gameInfo.Genres.Select(c => c.Description).ToList();

        foreach (var steamPrice in steamPrices)
        {
            var price = new Price(steamPrice.DiscountPercent, steamPrice.Final, steamPrice.Initial);
            
            game.PriceOverview.Add(steamPrice.Currency, price);
        }
        return game;
    }
}
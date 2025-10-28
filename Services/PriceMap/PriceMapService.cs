using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Services.Currencies;
using SOC_SteamPM_BE.Services.Steam;

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
    
    private readonly ILogger<PriceMapService> _logger;
    
    public PriceMapService(IEngineDataManager dataManager, ISteamApiService steamApi, ICurrencyService currencyService, ILogger<PriceMapService> logger)
    {
        _dataManager = dataManager;
        _steamApi = steamApi;
        _currencyService = currencyService;
        _logger = logger;
    }
    
    
    public async Task<GameInfo> GetGameInfoAndPrices(int appId, string currency = "EUR")
    {
        // Zkontroluje zda hra není náhodou v paměti, pokud je, tak jí to vrátí objekt bez converted cen
        // První se fetchne hra se všemi informacemi
        // Následně se nafetchují ceny a dostaneš list objektů SteamPrice
        // Ty zpracuješ a přidáš do GameInfo, bez converted ceny <= tenhle objekt se uloží do paměti jako "game-appId"
        // Pak tenhle objekt pošleš do CurrencyService, která objekt naplní converted cenami
        // Finální objekt vrátíš controlleru jako výsledek
        try
        {
            if (!_dataManager.TryGetCachedPriceMap($"game-{appId}", out var gameInfo))
            {
                string[] ccs = { "us","ae", "au", "br", "ca", "ch", "cl", "cn", "co", "cr", "cz", "gb", "il", "id", "in", "jp", "kr", "kw", "kz", "mx", "my", "no", "nz", "pe", "ph", "pl", "qa", "ru", "sa", "sg", "th", "tw", "ua", "ar", "dz", "np", "am", "uy", "vn", "za"};

                gameInfo = GameObjectBuilder(await _steamApi.FetchGameById(appId, "cz"), await _steamApi.FetchGamePrices(appId, ccs));
            
                // uložení do paměti
                _dataManager.SetCachedPriceMap($"game-{appId}", gameInfo);
            }
            
            await _currencyService.ConvertPrices(gameInfo.PriceOverview, currency);
            
            return gameInfo;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
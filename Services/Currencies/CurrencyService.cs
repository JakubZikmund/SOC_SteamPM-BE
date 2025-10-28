using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Models;

namespace SOC_SteamPM_BE.Services.Currencies;

public interface ICurrencyService
{
    public Task ConvertPrices(Dictionary<string, Price> pricesToConvert, string currency);   
}   

public class CurrencyService : ICurrencyService
{
    private readonly ILogger<CurrencyService> _logger;
    private readonly IEngineDataManager _dataManager;
    private readonly ICurrencyApiService _currencyApiService;
    
    public CurrencyService(ILogger<CurrencyService> logger, IEngineDataManager dataManager, ICurrencyApiService currencyApiService)
    {
        _logger = logger;
        _dataManager = dataManager;
        _currencyApiService = currencyApiService;
    }

    public async Task ConvertPrices(Dictionary<string, Price> pricesToConvert, string currency)
    {
        try
        {
            _logger.LogInformation("Converting prices to {Currency}", currency);
            if (!_dataManager.TryGetCachedCurrency($"curr-{currency}", out var currData))
            {
                currData = await _currencyApiService.FetchCurrency(currency);
            
                // uložení do paměti
                _dataManager.SetCachedCurrency($"curr-{currData.BaseCurrency}", currData);
            }
            
            foreach (var keyValuePair in pricesToConvert)
            {
                ConvertPrice(keyValuePair.Value, keyValuePair.Key, currData);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("An error occurred while proccesing currencies.", e);
        }
    }

    void ConvertPrice(Price price, string currentCurr, CurrencyModel currData)
    {
        price.ConvertedInitial = price.Initial/currData.ConversionalRates[currentCurr];
        price.ConvertedFinal = price.Final/currData.ConversionalRates[currentCurr];
    }
    
}
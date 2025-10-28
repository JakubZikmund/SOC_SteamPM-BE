using SOC_SteamPM_BE.Exceptions;
using SOC_SteamPM_BE.Managers;
using SOC_SteamPM_BE.Models;
using SOC_SteamPM_BE.Utils;

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
            if (!_dataManager.TryGetCachedCurrency(CacheKeys.Currency(currency), out var currData))
            {
                currData = await _currencyApiService.FetchCurrency(currency);
            
                // uložení do paměti
                _dataManager.SetCachedCurrency(CacheKeys.Currency(currency), currData);
            }
            
            foreach (var keyValuePair in pricesToConvert)
            {
                ConvertPrice(keyValuePair.Value, keyValuePair.Key, currData, currency);
            }
        }
        catch (InvalidCurrencyException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error while processing currencies for {Currency}", currency);
            throw new Exception("An error occurred while proccesing currencies.", e);
        }
    }

    void ConvertPrice(Price price, string currentCurr, CurrencyModel currData, string targetCurrency)
    {
        var usdRegions = new[] { "USD-LATAM", "USD-CIS", "USD-SASIA", "USD-MENA" };
        string lookupCurrency = usdRegions.Contains(currentCurr) ? "USD" : currentCurr;
        
        if (!currData.ConversionalRates.ContainsKey(lookupCurrency))
        {
            _logger.LogError("Currency {Currency} not found in conversion rates for target currency {TargetCurrency}", lookupCurrency, targetCurrency);
            throw new InvalidCurrencyException(targetCurrency, $"Unable to convert from {lookupCurrency} to {targetCurrency}. Currency code not found in conversion rates.");
        }
        
        price.ConvertedInitial = price.Initial / currData.ConversionalRates[lookupCurrency];
        price.ConvertedFinal = price.Final / currData.ConversionalRates[lookupCurrency];
    }
    
}
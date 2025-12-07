namespace SOC_SteamPM_BE.Models;

public class SteamApiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string AllGamesUrl { get; set; } = string.Empty;
    public string GameAllInfo { get; set; } = string.Empty;
    public string GamePriceInfo { get; set; } = string.Empty;
    public string WishlistUrl { get; set; } = string.Empty;
    public string[] CountryCodes { get; set; }
}

public class DataStorageSettings
{
    public string FolderPath { get; set; } = string.Empty;
    public bool LoadDataFromFileOnStartup { get; set; } = false;
}

public class CurrencySettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class WishlistSettings
{
    public int PageSize { get; set; } = 5;
}
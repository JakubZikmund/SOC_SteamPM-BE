namespace SOC_SteamPM_BE.Constants;

/// <summary>
/// Constants for input validation across the application
/// </summary>
public static class ValidationConstants
{
    // AppId validation
    public const int MinAppId = 1;
    public const int MaxAppId = int.MaxValue;
    public const string AppIdErrorMessage = "AppId must be a positive integer.";
    
    // Currency validation
    public const int CurrencyLength = 3;
    public const string CurrencyPattern = @"^[A-Z]{3}$";
    public const string CurrencyErrorMessage = "Currency code must be exactly 3 uppercase letters (e.g., EUR, USD, CZK).";
    
    // Search validation
    public const int MinSearchLength = 1;
    public const int MaxSearchLength = 200;
    public const string SearchErrorMessage = "Search term must be between 1 and 200 characters.";
    
    // Page validation
    public const int MinPage = 1;
    public const int MaxPage = int.MaxValue;
    public const string PageErrorMessage = "Page must be a positive integer.";
}


namespace SOC_SteamPM_BE.Exceptions;

public class InvalidCurrencyException : Exception
{
    public string CurrencyCode { get; }

    public InvalidCurrencyException(string currencyCode) 
        : base($"Invalid currency code: '{currencyCode}'. Please provide a valid currency code.")
    {
        CurrencyCode = currencyCode;
    }

    public InvalidCurrencyException(string currencyCode, string message) 
        : base(message)
    {
        CurrencyCode = currencyCode;
    }

    public InvalidCurrencyException(string currencyCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        CurrencyCode = currencyCode;
    }
}


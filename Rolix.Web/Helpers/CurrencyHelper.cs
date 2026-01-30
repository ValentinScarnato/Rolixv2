namespace Rolix.Web.Helpers;

public static class CurrencyHelper
{
    private const decimal EUR_TO_CHF_RATE = 0.95m; // 1 EUR = 0.95 CHF
    
    public static decimal ConvertPrice(decimal priceInCHF, string currency)
    {
        if (currency == "EUR")
        {
            return priceInCHF / EUR_TO_CHF_RATE;
        }
        return priceInCHF; // CHF par défaut
    }
    
    public static string FormatPrice(decimal priceInCHF, string currency)
    {
        var convertedPrice = ConvertPrice(priceInCHF, currency);
        
        if (currency == "EUR")
        {
            return convertedPrice.ToString("N0", System.Globalization.CultureInfo.CreateSpecificCulture("fr-FR")) + " €";
        }
        else
        {
            return convertedPrice.ToString("N0", System.Globalization.CultureInfo.CreateSpecificCulture("fr-CH")) + " CHF";
        }
    }
    
    public static string GetCurrencySymbol(string currency)
    {
        return currency == "EUR" ? "€" : "CHF";
    }
}

namespace Rolix.Web.Helpers;

/// <summary>
/// Utilitaire de conversion et formatage des devises.
/// Supporte la conversion entre CHF (Franc Suisse) et EUR (Euro).
/// </summary>
public static class CurrencyHelper
{
    /// <summary>
    /// Taux de conversion EUR vers CHF (1 EUR = 0.95 CHF)
    /// </summary>
    private const decimal EUR_TO_CHF_RATE = 0.95m; // 1 EUR = 0.95 CHF
    
    /// <summary>
    /// Convertit un prix de CHF vers la devise demandée.
    /// </summary>
    /// <param name="priceInCHF">Prix en Francs Suisses</param>
    /// <param name="currency">Code de devise ("CHF" ou "EUR")</param>
    /// <returns>Prix converti dans la devise demandée</returns>
    public static decimal ConvertPrice(decimal priceInCHF, string currency)
    {
        if (currency == "EUR")
        {
            return priceInCHF / EUR_TO_CHF_RATE;
        }
        return priceInCHF; // CHF par défaut
    }
    
    /// <summary>
    /// Convertit et formate un prix avec le symbole de devise approprié.
    /// </summary>
    /// <param name="priceInCHF">Prix en Francs Suisses</param>
    /// <param name="currency">Code de devise ("CHF" ou "EUR")</param>
    /// <returns>Prix formaté avec le symbole de devise (ex: "1 500 €" ou "1 500 CHF")</returns>
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
    
    /// <summary>
    /// Retourne le symbole de devise pour un code de devise donné.
    /// </summary>
    /// <param name="currency">Code de devise ("CHF" ou "EUR")</param>
    /// <returns>Symbole de devise ("€" pour EUR, "CHF" pour CHF)</returns>
    public static string GetCurrencySymbol(string currency)
    {
        return currency == "EUR" ? "€" : "CHF";
    }
}

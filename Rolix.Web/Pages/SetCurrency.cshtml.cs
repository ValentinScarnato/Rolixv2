using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Models;

namespace Rolix.Web.Pages;

public class SetCurrencyModel : PageModel
{
    public IActionResult OnGet(string currency, string? returnUrl)
    {
        if (currency == "CHF" || currency == "EUR")
        {
            HttpContext.Session.SetString(SessionKeys.Currency, currency);
        }
        
        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToPage("/Index");
    }
}

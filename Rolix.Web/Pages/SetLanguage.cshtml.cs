using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Models;

namespace Rolix.Web.Pages;

public class SetLanguageModel : PageModel
{
    public IActionResult OnGet(string language, string? returnUrl)
    {
        if (language == "fr" || language == "en")
        {
            HttpContext.Session.SetString(SessionKeys.Language, language);
        }
        
        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToPage("/Index");
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Models;
using Rolix.Web.Services;

namespace Rolix.Web.Pages.Products;

public class DetailsModel : PageModel
{
    private readonly ProductService _productService;
    private readonly QuoteService _quoteService;

    public Product? Product { get; set; }

    public bool HasPendingQuote { get; private set; }

    [BindProperty]
    public string? Comment { get; set; }

    public DetailsModel(ProductService productService, QuoteService quoteService)
    {
        _productService = productService;
        _quoteService = quoteService;
    }

    public IActionResult OnGet(Guid id)
    {
        Product = _productService.GetById(id);

        if (Product == null)
            return NotFound();

        var contactId = HttpContext.Session.GetString(SessionKeys.ContactId);
        if (Guid.TryParse(contactId, out var contactGuid))
        {
            try
            {
                HasPendingQuote = _quoteService.HasPendingQuoteForProduct(contactGuid, Product.Id);
            }
            catch
            {
                HasPendingQuote = false;
            }
        }

        return Page();
    }

    public IActionResult OnPostQuote(Guid id)
    {
        Product = _productService.GetById(id);

        if (Product == null)
            return NotFound();

        var contactId = HttpContext.Session.GetString(SessionKeys.ContactId);
        var contactEmail = HttpContext.Session.GetString(SessionKeys.ContactEmail);

        if (string.IsNullOrEmpty(contactId))
        {
            var returnUrl = Url.Page("/Products/Details", new { id });
            return RedirectToPage("/Account/Index", new { returnUrl });
        }

        QuoteCreationResult result;
        try
        {
            result = _quoteService.CreateQuote(Guid.Parse(contactId), Product, Comment, contactEmail);
        }
        catch (Exception ex)
        {
            TempData["QuoteWarning"] = $"Impossible d'envoyer votre demande de devis pour le moment : {ex.Message}";
            return RedirectToPage(new { id });
        }

        if (result.QuoteId == Guid.Empty)
        {
            TempData["QuoteWarning"] = result.Warning ?? "Vous avez déjà un devis en attente pour ce produit.";
            return RedirectToPage(new { id });
        }

        TempData["QuoteSuccess"] = "Votre devis a été créé.";
        if (!string.IsNullOrWhiteSpace(result.Warning))
        {
            TempData["QuoteWarning"] = result.Warning;
        }

        return RedirectToPage(new { id });
    }
}

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

        _quoteService.CreateQuoteRequest(Guid.Parse(contactId), Product, contactEmail);

        TempData["QuoteSuccess"] = "Votre demande de devis a été transmise. Notre équipe vous contactera rapidement.";

        return RedirectToPage(new { id });
    }
}

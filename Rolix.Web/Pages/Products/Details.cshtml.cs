using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Models;
using Rolix.Web.Services;

namespace Rolix.Web.Pages.Products;

public class DetailsModel : PageModel
{
    private readonly ProductService _productService;

    public Product? Product { get; set; }

    public DetailsModel(ProductService productService)
    {
        _productService = productService;
    }

    public IActionResult OnGet(Guid id)
    {
        Product = _productService.GetById(id);

        if (Product == null)
            return NotFound();

        return Page();
    }
}

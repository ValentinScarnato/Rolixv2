using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Models;
using Rolix.Web.Services;

namespace Rolix.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;

        public IndexModel(ProductService productService)
        {
            _productService = productService;
        }

        // Cette liste ne contiendra QUE les 2 montres pour l'accueil
        public List<Product> TopProducts { get; set; } = new();

        public void OnGet()
        {
            // On demande spécifiquement 2 produits
            TopProducts = _productService.GetTopExpensiveProducts(2);
        }
    }
}
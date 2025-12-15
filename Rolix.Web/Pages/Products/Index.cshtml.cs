using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Models;
using Rolix.Web.Services;

namespace Rolix.Web.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;

        public IndexModel(ProductService productService)
        {
            _productService = productService;
        }

        public List<Product> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; }

        // NOUVEAU : On récupère l'ID de la catégorie (peut être null si "Tout")
        [BindProperty(SupportsGet = true)]
        public int? Category { get; set; }

        public void OnGet()
        {
            // On passe les 3 paramètres au service
            Products = _productService.GetFiltered(SearchString, SortOrder, Category);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Models;
using Rolix.Web.Services;
using System.Linq; // Assure-toi d'avoir ça

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
        public List<Product> Families { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? FamilyId { get; set; }

        public void OnGet()
        {
            Families = _productService.GetFamilies();

            // 1. On récupère tout
            var allProducts = _productService.GetFiltered(SearchString ?? string.Empty, SortOrder ?? "name_asc", FamilyId);

            // 2. On FILTRE : On garde ceux qui ont une image non nulle et non vide
            Products = allProducts
                        .Where(p => !string.IsNullOrEmpty(p.ImageBase64))
                        .ToList();
        }
    }
}
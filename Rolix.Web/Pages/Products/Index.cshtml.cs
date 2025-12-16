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
            Products = _productService.GetFiltered(SearchString ?? string.Empty, SortOrder ?? "name_asc", FamilyId);
        }
    }
}

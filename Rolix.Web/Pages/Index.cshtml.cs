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

        public List<Product> TopProducts { get; set; } = new();

        public void OnGet()
        {
            TopProducts = _productService.GetTopExpensiveProducts(3);
        }
    }
}
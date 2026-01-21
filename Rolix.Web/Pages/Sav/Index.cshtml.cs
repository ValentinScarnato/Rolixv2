using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Models;
using Rolix.Web.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rolix.Web.Pages.Sav
{
    public class IndexModel : PageModel
    {
        private readonly SavService _savService;
        private readonly ProductService _productService;

        public IndexModel(SavService savService, ProductService productService)
        {
            _savService = savService;
            _productService = productService;
        }

        public List<SavRequest> SavRequests { get; set; } = new();
        public List<Product> Products { get; set; } = new();

        [BindProperty]
        public Guid ProductId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Le diagnostique est requis")]
        public string Diagnostic { get; set; } = string.Empty;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var contactIdStr = HttpContext.Session.GetString("ContactId");
            if (!Guid.TryParse(contactIdStr, out var contactId))
                return RedirectToPage("/Account/Index");

            SavRequests = _savService.GetSavRequestsForContact(contactId);
            Products = _productService.GetAll();

            return Page();
        }

        public IActionResult OnPost()
        {
            var contactIdStr = HttpContext.Session.GetString("ContactId");
            if (!Guid.TryParse(contactIdStr, out var contactId))
                return RedirectToPage("/Account/Index");

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Veuillez remplir tous les champs requis.";
                SavRequests = _savService.GetSavRequestsForContact(contactId);
                Products = _productService.GetAll();
                return Page();
            }

            try
            {
                var product = _productService.GetById(ProductId);
                if (product == null)
                {
                    ErrorMessage = "Produit introuvable.";
                    SavRequests = _savService.GetSavRequestsForContact(contactId);
                    Products = _productService.GetAll();
                    return Page();
                }

                _savService.CreateSavRequest(contactId, ProductId, product.Name, Diagnostic);
                SuccessMessage = "Votre demande SAV a été créée avec succès. Nous vous contacterons sous peu.";

                return RedirectToPage();
            }
            catch (Exception)
            {
                ErrorMessage = "Une erreur est survenue lors de la création de votre demande SAV.";
                SavRequests = _savService.GetSavRequestsForContact(contactId);
                Products = _productService.GetAll();
                return Page();
            }
        }
    }
}

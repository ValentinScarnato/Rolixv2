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
        private readonly InvoiceService _invoiceService;

        public IndexModel(SavService savService, InvoiceService invoiceService)
        {
            _savService = savService;
            _invoiceService = invoiceService;
        }

        public List<SavRequest> SavRequests { get; set; } = new();
        public List<PurchasedProduct> PurchasedProducts { get; set; } = new();

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

            LoadData(contactId);

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
                LoadData(contactId);
                return Page();
            }

            try
            {
                // Vérifier si le produit a bien été acheté par le client
                var purchasedProducts = _invoiceService.GetPurchasedProductsForContact(contactId);
                var purchasedProduct = purchasedProducts.FirstOrDefault(p => p.ProductId == ProductId);

                if (purchasedProduct == null)
                {
                    ErrorMessage = "Produit non trouvé dans vos achats ou erreur de référence.";
                    LoadData(contactId);
                    return Page();
                }

                _savService.CreateSavRequest(contactId, ProductId, purchasedProduct.ProductName, Diagnostic);
                SuccessMessage = "Votre demande SAV a été créée avec succès. Nous vous contacterons sous peu.";

                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                LoadData(contactId);
                return Page();
            }
            catch (Exception ex)
            {
                var fullError = ex.Message;
                if (ex.InnerException != null)
                {
                    fullError += " | Inner: " + ex.InnerException.Message;
                }
                ErrorMessage = $"Erreur technique : {fullError}";
                LoadData(contactId);
                return Page();
            }
        }

        public IActionResult OnPostRate(Guid id, int rating)
        {
            var contactIdStr = HttpContext.Session.GetString("ContactId");
            if (string.IsNullOrEmpty(contactIdStr))
                return RedirectToPage("/Account/Index");

            _savService.UpdateSavRequestRating(id, rating);

            return RedirectToPage();
        }

        private void LoadData(Guid contactId)
        {
            SavRequests = _savService.GetSavRequestsForContact(contactId);
            PurchasedProducts = _invoiceService.GetPurchasedProductsForContact(contactId);

            // Marquer les produits qui ont déjà une demande SAV
            // On peut le faire en mémoire ou via la méthode dédiée du service pour chaque produit
            // Pour optimiser, on peut regarder la liste des SavRequests chargées
            foreach (var product in PurchasedProducts)
            {
                product.HasSavRequest = SavRequests.Any(s => s.ProductId == product.ProductId);
            }
            
            // Si on veut être sûr à 100% (par ex si une demande terminée n'est pas chargée dans SavRequests mais compte quand même)
            // On pourrait appeler _savService.HasSavRequestForProduct pour chaque produit, mais c'est moins performant.
            // La méthode actuelle via la liste chargée est cohérente avec ce que l'utilisateur voit.
        }
    }
}

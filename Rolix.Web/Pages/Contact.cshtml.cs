using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace Rolix.Web.Pages
{
    public class ContactModel : PageModel
    {
        private readonly LeadService _leadService;

        public ContactModel(LeadService leadService)
        {
            _leadService = leadService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Le prénom est requis")]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Le nom est requis")]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Le sujet est requis")]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Le message est requis")]
        public string Message { get; set; } = string.Empty;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Veuillez remplir tous les champs requis.";
                return Page();
            }

            try
            {
                _leadService.CreateLead(FirstName, LastName, Email, Subject, Message);
                SuccessMessage = "Votre message a été envoyé avec succès. Nous vous répondrons sous 24-48h.";
                
                // Réinitialiser le formulaire
                ModelState.Clear();
                FirstName = string.Empty;
                LastName = string.Empty;
                Email = string.Empty;
                Subject = string.Empty;
                Message = string.Empty;

                return RedirectToPage();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Une erreur est survenue lors de l'envoi de votre message. Veuillez réessayer.";
                // Log l'erreur si nécessaire
                return Page();
            }
        }
    }
}

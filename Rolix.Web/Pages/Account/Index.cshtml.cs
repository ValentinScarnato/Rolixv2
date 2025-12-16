using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Rolix.Web.Models;
using Rolix.Web.Services;

namespace Rolix.Web.Pages.Account
{
    public class IndexModel : PageModel
    {
        private readonly ContactService _contactService;

        public IndexModel(ContactService contactService)
        {
            _contactService = contactService;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        [BindProperty]
        public RegisterInput Register { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.ContactId));

        public string? ContactName => HttpContext.Session.GetString(SessionKeys.ContactName);

        public string? ContactEmail => HttpContext.Session.GetString(SessionKeys.ContactEmail);

        public void OnGet()
        {
        }

        public IActionResult OnPostLogin()
        {
            ClearRegisterValidation();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var contact = _contactService.Authenticate(Input.Email, Input.LastName);

            if (contact == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou nom non reconnu dans Dataverse.");
                return Page();
            }

            SignIn(contact);

            return RedirectAfterSuccess("Connexion réussie.");
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Vous êtes déconnecté.";

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostRegister()
        {
            ClearLoginValidation();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = _contactService.GetByEmail(Register.Email);

            if (existing != null)
            {
                ModelState.AddModelError(string.Empty, "Un compte existe déjà avec cet email. Connectez-vous ou utilisez une autre adresse.");
                return Page();
            }

            var contact = _contactService.Create(new Contact
            {
                FirstName = Register.FirstName,
                LastName = Register.LastName,
                Email = Register.Email,
            });

            SignIn(contact);

            return RedirectAfterSuccess("Compte créé et connecté.");
        }

        private IActionResult RedirectAfterSuccess(string message)
        {
            TempData["Success"] = message;

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToPage();
        }

        private void SignIn(Contact contact)
        {
            HttpContext.Session.SetString(SessionKeys.ContactId, contact.Id.ToString());
            HttpContext.Session.SetString(SessionKeys.ContactName, contact.DisplayName);
            HttpContext.Session.SetString(SessionKeys.ContactEmail, contact.Email ?? string.Empty);
        }

        private void ClearRegisterValidation()
        {
            ModelState.Remove($"{nameof(Register)}.{nameof(Register.FirstName)}");
            ModelState.Remove($"{nameof(Register)}.{nameof(Register.LastName)}");
            ModelState.Remove($"{nameof(Register)}.{nameof(Register.Email)}");
        }

        private void ClearLoginValidation()
        {
            ModelState.Remove($"{nameof(Input)}.{nameof(Input.Email)}");
            ModelState.Remove($"{nameof(Input)}.{nameof(Input.LastName)}");
        }
    }

    public class LoginInput
    {
        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis.")]
        public string LastName { get; set; } = string.Empty;
    }

    public class RegisterInput
    {
        [Required(ErrorMessage = "Le prénom est requis.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        public string Email { get; set; } = string.Empty;
    }
}

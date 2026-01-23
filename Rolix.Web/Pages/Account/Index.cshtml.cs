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
        private readonly QuoteService _quoteService;

        public IndexModel(ContactService contactService, QuoteService quoteService)
        {
            _contactService = contactService;
            _quoteService = quoteService;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        [BindProperty]
        public RegisterInput Register { get; set; } = new();

        [BindProperty]
        public UpdateInput Update { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.ContactId));

        public string? ContactName => HttpContext.Session.GetString(SessionKeys.ContactName);

        public string? ContactEmail => HttpContext.Session.GetString(SessionKeys.ContactEmail);

        public string? ContactUsername => HttpContext.Session.GetString(SessionKeys.ContactUsername);

        public List<QuoteRequestSummary> QuoteRequests { get; set; } = new();

        public List<PendingQuoteSummary> PendingQuotes { get; set; } = new();

        public void OnGet()
        {
            if (!IsAuthenticated)
            {
                return;
            }

            var contactIdString = HttpContext.Session.GetString(SessionKeys.ContactId);
            if (!Guid.TryParse(contactIdString, out var contactId))
            {
                return;
            }

            try
            {
                var contact = _contactService.GetById(contactId);
                if (contact == null)
                {
                    return;
                }

                Update.FullName = contact.FullName ?? contact.DisplayName;
                Update.Email = contact.Email ?? string.Empty;

                QuoteRequests = _quoteService.GetQuoteRequestsForContact(contactId);
                PendingQuotes = _quoteService.GetPendingQuotesForContact(contactId);
            }
            catch
            {
                // On évite de bloquer l'affichage de la page si Dataverse est indisponible
            }
        }

        public IActionResult OnPostLogin()
        {
            ModelState.Clear();
            if (!TryValidateModel(Input, nameof(Input)))
            {
                return Page();
            }

            var contact = _contactService.Authenticate(Input.Username, Input.Password);

            if (contact == null)
            {
                ModelState.AddModelError(string.Empty, "Identifiants invalides.");
                return Page();
            }

            HttpContext.Session.SetString(SessionKeys.ContactId, contact.Id.ToString());
            HttpContext.Session.SetString(SessionKeys.ContactName, contact.FullName ?? contact.DisplayName);
            HttpContext.Session.SetString(SessionKeys.ContactEmail, contact.Email ?? string.Empty);
            HttpContext.Session.SetString(SessionKeys.ContactUsername, contact.Username ?? string.Empty);

            TempData["Success"] = "Connexion réussie.";

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToPage();
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

        public IActionResult OnGetLogout()
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
            ModelState.Clear();
            if (!TryValidateModel(Register, nameof(Register)))
            {
                return Page();
            }

            var existingEmail = _contactService.GetByEmail(Register.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError(string.Empty, "Un compte existe déjà avec cet email dans Dataverse.");
                return Page();
            }

            var existingUsername = _contactService.GetByUsername(Register.Username);
            if (existingUsername != null)
            {
                ModelState.AddModelError(string.Empty, "Un compte existe déjà avec ce nom d'utilisateur.");
                return Page();
            }

            var contactId = _contactService.Create(Register.FullName, Register.Email, Register.Username, Register.Password);
            var created = _contactService.GetById(contactId);

            HttpContext.Session.SetString(SessionKeys.ContactId, contactId.ToString());
            HttpContext.Session.SetString(SessionKeys.ContactName, created?.FullName ?? Register.FullName);
            HttpContext.Session.SetString(SessionKeys.ContactEmail, Register.Email ?? string.Empty);
            HttpContext.Session.SetString(SessionKeys.ContactUsername, Register.Username ?? string.Empty);

            TempData["Success"] = "Compte créé avec succès.";

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostUpdate()
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage();
            }

            var contactIdString = HttpContext.Session.GetString(SessionKeys.ContactId);
            if (!Guid.TryParse(contactIdString, out var contactId))
            {
                HttpContext.Session.Clear();
                return RedirectToPage();
            }

            ModelState.Clear();
            if (!TryValidateModel(Update, nameof(Update)))
            {
                return Page();
            }

            _contactService.Update(contactId, Update.FullName, Update.Email);
            var updated = _contactService.GetById(contactId);

            HttpContext.Session.SetString(SessionKeys.ContactName, updated?.FullName ?? Update.FullName);
            HttpContext.Session.SetString(SessionKeys.ContactEmail, Update.Email ?? string.Empty);

            TempData["Success"] = "Compte mis à jour.";

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToPage();
        }
    }

    public class LoginInput
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterInput
    {
        [Required(ErrorMessage = "Le nom complet est requis.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+$", ErrorMessage = "L'email doit contenir un @.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [MinLength(12, ErrorMessage = "Le mot de passe doit contenir au moins 12 caractères.")]
        [RegularExpression(@"^(?=.*[!@#$%^&*(),.?':;{}|<>_\-+=\[\]\\\/`~]).*$", ErrorMessage = "Le mot de passe doit contenir au moins un caractère spécial.")]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateInput
    {
        [Required(ErrorMessage = "Le nom complet est requis.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        public string Email { get; set; } = string.Empty;
    }
}

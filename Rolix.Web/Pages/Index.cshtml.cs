using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Models;
using Rolix.Web.Services;

namespace Rolix.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;
        private readonly ModificationService _modificationService;

        public IndexModel(ProductService productService, ModificationService modificationService)
        {
            _productService = productService;
            _modificationService = modificationService;
        }

        public List<Product> TopProducts { get; set; } = new();

        // Check if current user is a director
        public bool IsDirector => HttpContext.Session.GetInt32(SessionKeys.AccountRoleCode) == 1;

        // Hero section
        [BindProperty]
        public string Title { get; set; } = "Le Temps, Sublimé.";

        [BindProperty]
        public string Subtitle { get; set; } = "Perpétuer l'excellence depuis 1905";

        // Chapter I
        [BindProperty]
        public string Title1 { get; set; } = "Une Vision Perpétuelle";

        [BindProperty]
        public string Content1 { get; set; } = "Tout commence par une quête d'absolu. Fondée au début du XXe siècle, la Maison Rolix n'a eu de cesse de repousser les limites de la précision chronométrique. Plus qu'une montre, nous façonnons des instruments destinés à accompagner les explorateurs, les visionnaires et ceux qui façonnent le monde de demain.";

        // Quote section
        [BindProperty]
        public string Quote1 { get; set; } = "Nous ne construisons pas seulement des montres. Nous gardons la mesure de vos instants les plus précieux.";

        [BindProperty]
        public string Author1 { get; set; } = "Henri Rolix, Fondateur";

        // Chapter II
        [BindProperty]
        public string Title2 { get; set; } = "L'Or & La Main";

        [BindProperty]
        public string Content2 { get; set; } = "Dans le secret de nos ateliers genevois, chaque composant est poli, assemblé et contrôlé à la main. L'éclat de notre or 18 carats, fondu dans nos propres fonderies, est unique. Cette alchimie entre tradition artisanale et technologie de pointe donne naissance à une esthétique inaltérable.";

        // Gallery
        [BindProperty]
        public string Gallery { get; set; } = "Collection Prestige";

        // Maison section (About content)
        [BindProperty]
        public string MaisonTitle { get; set; } = "Nous ne fabriquons pas <br> le temps. <span class=\"text-gold\">Nous l'honorons.</span>";

        [BindProperty]
        public string MaisonBadge { get; set; } = "Depuis 1905";

        [BindProperty]
        public string LeadParagraph { get; set; } = "Chaque seconde est une victoire sur le chaos. Dans nos ateliers de Genève, le silence n'est brisé que par le tic-tac régulier de milliers de cœurs mécaniques qui se mettent à battre à l'unisson.";

        [BindProperty]
        public string Paragraph2 { get; set; } = "Rolix n'est pas née d'une étude de marché, mais de l'obsession d'un homme, Henri Rolix, qui refusait que l'élégance se fasse au détriment de la précision.";

        [BindProperty]
        public string Paragraph3 { get; set; } = "Aujourd'hui, nous sommes les gardiens d'un art ancien. Alors que le monde accélère, nous prenons le temps de polir chaque angle, d'ajuster chaque ressort, de tester chaque boîtier dans des conditions extrêmes.";

        [BindProperty]
        public string EditorialQuote { get; set; } = "Le luxe, c'est ce qui ne se voit pas, mais qui se ressent à chaque instant.";

        // Archives
        [BindProperty]
        public string ArchivesTitle { get; set; } = "Les Archives";

        [BindProperty]
        public string Archive1Text { get; set; } = "1920 — Le premier croquis";

        [BindProperty]
        public string Archive2Text { get; set; } = "1954 — Le Calibre 3200";

        [BindProperty]
        public string Archive3Text { get; set; } = "1980 — La main de l'homme";

        public void OnGet()
        {
            TopProducts = _productService.GetTopExpensiveProducts(3);

            // Load latest modification or use defaults
            try
            {
                var language = HttpContext.Session.GetString(SessionKeys.Language) ?? "fr";
                var languageCode = language == "en" ? 2 : 1;
                
                var latestMod = _modificationService.GetLatestModification(languageCode);
                if (latestMod != null)
                {
                    // Hero section
                    Title = latestMod.Title ?? Title;
                    Subtitle = latestMod.Subtitle ?? Subtitle;
                    
                    // Chapter I
                    Title1 = latestMod.Title1 ?? Title1;
                    Content1 = latestMod.Content1 ?? Content1;
                    
                    // Quote
                    Quote1 = latestMod.Quote1 ?? Quote1;
                    Author1 = latestMod.Author1 ?? Author1;
                    
                    // Chapter II
                    Title2 = latestMod.Title2 ?? Title2;
                    Content2 = latestMod.Content2 ?? Content2;
                    
                    // Gallery
                    Gallery = latestMod.Gallery ?? Gallery;
                    
                    // Maison section
                    MaisonTitle = latestMod.MaisonTitle ?? MaisonTitle;
                    MaisonBadge = latestMod.MaisonBadge ?? MaisonBadge;
                    LeadParagraph = latestMod.LeadParagraph ?? LeadParagraph;
                    Paragraph2 = latestMod.Paragraph2 ?? Paragraph2;
                    Paragraph3 = latestMod.Paragraph3 ?? Paragraph3;
                    EditorialQuote = latestMod.EditorialQuote ?? EditorialQuote;
                    
                    // Archives
                    ArchivesTitle = latestMod.ArchivesTitle ?? ArchivesTitle;
                    Archive1Text = latestMod.Archive1Text ?? Archive1Text;
                    Archive2Text = latestMod.Archive2Text ?? Archive2Text;
                    Archive3Text = latestMod.Archive3Text ?? Archive3Text;
                }
            }
            catch
            {
                // If Dataverse is unavailable or table is empty, use hardcoded defaults
            }
        }

        public IActionResult OnPostSave()
        {
            // Only directors can save modifications
            if (!IsDirector)
            {
                return Forbid();
            }

            try
            {
                var modification = new Modification
                {
                    // Hero section
                    Title = Title,
                    Subtitle = Subtitle,
                    
                    // Chapter I
                    Title1 = Title1,
                    Content1 = Content1,
                    
                    // Quote
                    Quote1 = Quote1,
                    Author1 = Author1,
                    
                    // Chapter II
                    Title2 = Title2,
                    Content2 = Content2,
                    
                    // Gallery
                    Gallery = Gallery,
                    
                    // Maison section
                    MaisonTitle = MaisonTitle,
                    MaisonBadge = MaisonBadge,
                    LeadParagraph = LeadParagraph,
                    Paragraph2 = Paragraph2,
                    Paragraph3 = Paragraph3,
                    EditorialQuote = EditorialQuote,
                    
                    // Archives
                    ArchivesTitle = ArchivesTitle,
                    Archive1Text = Archive1Text,
                    Archive2Text = Archive2Text,
                    Archive3Text = Archive3Text
                };

                var language = HttpContext.Session.GetString(SessionKeys.Language) ?? "fr";
                var languageCode = language == "en" ? 2 : 1;

                _modificationService.SaveModification(modification, languageCode);

                TempData["Success"] = "Modifications enregistrées avec succès.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Erreur lors de l'enregistrement : {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Détails: {ex.InnerException.Message}";
                }
                ModelState.AddModelError(string.Empty, errorMessage);
                TopProducts = _productService.GetTopExpensiveProducts(3);
                return Page();
            }
        }
    }
}
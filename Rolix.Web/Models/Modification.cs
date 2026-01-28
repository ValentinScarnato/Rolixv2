namespace Rolix.Web.Models;

public class Modification
{
    public Guid Id { get; set; }
    public string? ModificationName { get; set; }
    public DateTime? ModificationDate { get; set; }
    
    public int? LanguageCode { get; set; }

    // Hero section
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    
    // Chapter I
    public string? Title1 { get; set; }
    public string? Content1 { get; set; }
    
    // Quote section
    public string? Quote1 { get; set; }
    public string? Text1 { get; set; }
    public string? Author1 { get; set; }
    
    // Chapter II
    public string? Title2 { get; set; }
    public string? Content2 { get; set; }
    
    // Gallery
    public string? Gallery { get; set; }
    
    // Maison section (About content)
    public string? MaisonTitle { get; set; }
    public string? MaisonBadge { get; set; }
    public string? LeadParagraph { get; set; }
    public string? Paragraph2 { get; set; }
    public string? Paragraph3 { get; set; }
    public string? EditorialQuote { get; set; }
    
    // Archives
    public string? ArchivesTitle { get; set; }
    public string? Archive1Text { get; set; }
    public string? Archive2Text { get; set; }
    public string? Archive3Text { get; set; }
}

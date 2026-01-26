using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rolix.Web.Models;

namespace Rolix.Web.Services;

public class ModificationService
{
    private readonly DataverseService _dataverse;

    public ModificationService(DataverseService dataverse)
    {
        _dataverse = dataverse;
    }

    public Modification? GetLatestModification()
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("rlx_modification")
        {
            ColumnSet = new ColumnSet(
                "rlx_modificationid",
                "rlx_modificationnom",
                "rlx_datedemodification",
                "rlx_title",
                "rlx_subtitle",
                "rlx_title1",
                "rlx_content1",
                "rlx_quote1",
                "rlx_text1",
                "rlx_author1",
                "rlx_title2",
                "rlx_content2",
                "rlx_gallery",
                "rlx_section",
                // Maison section fields
                "rlx_maisontitle",
                "rlx_maisonbadge",
                "rlx_leadparagraph",
                "rlx_paragraph2",
                "rlx_paragraph3",
                "rlx_editorialquote",
                // Statistics fields
                "rlx_stat1number",
                "rlx_stat1label",
                "rlx_stat2number",
                "rlx_stat2label",
                "rlx_stat3number",
                "rlx_stat3label",
                // Archives fields
                "rlx_archivestitle",
                "rlx_archive1text",
                "rlx_archive2text",
                "rlx_archive3text"
            ),
            TopCount = 1
        };

        query.Orders.Add(new OrderExpression("rlx_datedemodification", OrderType.Descending));

        var result = client.RetrieveMultiple(query);
        var entity = result.Entities.FirstOrDefault();

        if (entity == null)
        {
            return null;
        }

        return MapModification(entity);
    }

    public Guid SaveModification(Modification modification)
    {
        var client = _dataverse.GetClient();

        // Check if a modification already exists
        var query = new QueryExpression("rlx_modification")
        {
            ColumnSet = new ColumnSet("rlx_modificationid"),
            TopCount = 1
        };
        query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));

        var result = client.RetrieveMultiple(query);
        var existingEntity = result.Entities.FirstOrDefault();

        Entity entity;
        bool isUpdate = false;

        if (existingEntity != null)
        {
            // Update existing record
            entity = new Entity("rlx_modification", existingEntity.Id);
            isUpdate = true;
        }
        else
        {
            // Create new record
            entity = new Entity("rlx_modification");
        }

        // Set all fields
        entity["rlx_modificationnom"] = modification.ModificationName;
        entity["rlx_datedemodification"] = DateTime.Now;
        
        // Hero section
        entity["rlx_title"] = modification.Title;
        entity["rlx_subtitle"] = modification.Subtitle;
        
        // Chapter I
        entity["rlx_title1"] = modification.Title1;
        entity["rlx_content1"] = modification.Content1;
        
        // Quote
        entity["rlx_quote1"] = modification.Quote1;
        entity["rlx_text1"] = modification.Text1;
        entity["rlx_author1"] = modification.Author1;
        
        // Chapter II
        entity["rlx_title2"] = modification.Title2;
        entity["rlx_content2"] = modification.Content2;
        
        // Gallery
        entity["rlx_gallery"] = modification.Gallery;
        entity["rlx_section"] = modification.Section;
        
        // Maison section
        entity["rlx_maisontitle"] = modification.MaisonTitle;
        entity["rlx_maisonbadge"] = modification.MaisonBadge;
        entity["rlx_leadparagraph"] = modification.LeadParagraph;
        entity["rlx_paragraph2"] = modification.Paragraph2;
        entity["rlx_paragraph3"] = modification.Paragraph3;
        entity["rlx_editorialquote"] = modification.EditorialQuote;
        
        // Statistics
        entity["rlx_stat1number"] = modification.Stat1Number;
        entity["rlx_stat1label"] = modification.Stat1Label;
        entity["rlx_stat2number"] = modification.Stat2Number;
        entity["rlx_stat2label"] = modification.Stat2Label;
        entity["rlx_stat3number"] = modification.Stat3Number;
        entity["rlx_stat3label"] = modification.Stat3Label;
        
        // Archives
        entity["rlx_archivestitle"] = modification.ArchivesTitle;
        entity["rlx_archive1text"] = modification.Archive1Text;
        entity["rlx_archive2text"] = modification.Archive2Text;
        entity["rlx_archive3text"] = modification.Archive3Text;

        if (isUpdate)
        {
            client.Update(entity);
            return entity.Id;
        }
        else
        {
            return client.Create(entity);
        }
    }

    private static Modification MapModification(Entity entity)
    {
        return new Modification
        {
            Id = entity.Id,
            ModificationName = entity.GetAttributeValue<string>("rlx_modificationnom"),
            ModificationDate = entity.GetAttributeValue<DateTime?>("rlx_datedemodification"),
            
            // Hero section
            Title = entity.GetAttributeValue<string>("rlx_title"),
            Subtitle = entity.GetAttributeValue<string>("rlx_subtitle"),
            
            // Chapter I
            Title1 = entity.GetAttributeValue<string>("rlx_title1"),
            Content1 = entity.GetAttributeValue<string>("rlx_content1"),
            
            // Quote
            Quote1 = entity.GetAttributeValue<string>("rlx_quote1"),
            Text1 = entity.GetAttributeValue<string>("rlx_text1"),
            Author1 = entity.GetAttributeValue<string>("rlx_author1"),
            
            // Chapter II
            Title2 = entity.GetAttributeValue<string>("rlx_title2"),
            Content2 = entity.GetAttributeValue<string>("rlx_content2"),
            
            // Gallery
            Gallery = entity.GetAttributeValue<string>("rlx_gallery"),
            Section = entity.GetAttributeValue<string>("rlx_section"),
            
            // Maison section
            MaisonTitle = entity.GetAttributeValue<string>("rlx_maisontitle"),
            MaisonBadge = entity.GetAttributeValue<string>("rlx_maisonbadge"),
            LeadParagraph = entity.GetAttributeValue<string>("rlx_leadparagraph"),
            Paragraph2 = entity.GetAttributeValue<string>("rlx_paragraph2"),
            Paragraph3 = entity.GetAttributeValue<string>("rlx_paragraph3"),
            EditorialQuote = entity.GetAttributeValue<string>("rlx_editorialquote"),
            
            // Statistics
            Stat1Number = entity.GetAttributeValue<string>("rlx_stat1number"),
            Stat1Label = entity.GetAttributeValue<string>("rlx_stat1label"),
            Stat2Number = entity.GetAttributeValue<string>("rlx_stat2number"),
            Stat2Label = entity.GetAttributeValue<string>("rlx_stat2label"),
            Stat3Number = entity.GetAttributeValue<string>("rlx_stat3number"),
            Stat3Label = entity.GetAttributeValue<string>("rlx_stat3label"),
            
            // Archives
            ArchivesTitle = entity.GetAttributeValue<string>("rlx_archivestitle"),
            Archive1Text = entity.GetAttributeValue<string>("rlx_archive1text"),
            Archive2Text = entity.GetAttributeValue<string>("rlx_archive2text"),
            Archive3Text = entity.GetAttributeValue<string>("rlx_archive3text"),
        };
    }
}

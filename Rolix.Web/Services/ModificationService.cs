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

    public Modification? GetLatestModification(int languageCode)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("rlx_modification")
        {
            ColumnSet = new ColumnSet(
                "rlx_modificationid",
                "rlx_modificationnom",
                "rlx_datedemodification",
                "rlx_langue",
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
                // Maison section fields
                "rlx_maisontitle",
                "rlx_maisonbadge",
                "rlx_leadparagraph",
                "rlx_paragraph2",
                "rlx_paragraph3",
                "rlx_editorialquote",
                // Archives fields
                "rlx_archivestitle",
                "rlx_archive1text",
                "rlx_archive2text",
                "rlx_archive3text"
            ),
            TopCount = 1
        };

        query.Criteria.AddCondition("rlx_langue", ConditionOperator.Equal, languageCode);

        query.Orders.Add(new OrderExpression("rlx_datedemodification", OrderType.Descending));

        var result = client.RetrieveMultiple(query);
        var entity = result.Entities.FirstOrDefault();

        if (entity == null)
        {
            return null;
        }

        return MapModification(entity);
    }

    public Guid SaveModification(Modification modification, int languageCode)
    {
        var client = _dataverse.GetClient();

        // Check if a modification already exists for this language - get the most recently created one
        var query = new QueryExpression("rlx_modification")
        {
            ColumnSet = new ColumnSet("rlx_modificationid"),
            TopCount = 1
        };
        query.Criteria.AddCondition("rlx_langue", ConditionOperator.Equal, languageCode);
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
        entity["rlx_datedemodification"] = DateTime.Now;
        entity["rlx_langue"] = new OptionSetValue(languageCode);
        
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
        
        // Maison section
        entity["rlx_maisontitle"] = modification.MaisonTitle;
        entity["rlx_maisonbadge"] = modification.MaisonBadge;
        entity["rlx_leadparagraph"] = modification.LeadParagraph;
        entity["rlx_paragraph2"] = modification.Paragraph2;
        entity["rlx_paragraph3"] = modification.Paragraph3;
        entity["rlx_editorialquote"] = modification.EditorialQuote;
        
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
            LanguageCode = entity.GetAttributeValue<OptionSetValue>("rlx_langue")?.Value,
            
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
            
            // Maison section
            MaisonTitle = entity.GetAttributeValue<string>("rlx_maisontitle"),
            MaisonBadge = entity.GetAttributeValue<string>("rlx_maisonbadge"),
            LeadParagraph = entity.GetAttributeValue<string>("rlx_leadparagraph"),
            Paragraph2 = entity.GetAttributeValue<string>("rlx_paragraph2"),
            Paragraph3 = entity.GetAttributeValue<string>("rlx_paragraph3"),
            EditorialQuote = entity.GetAttributeValue<string>("rlx_editorialquote"),
            
            // Archives
            ArchivesTitle = entity.GetAttributeValue<string>("rlx_archivestitle"),
            Archive1Text = entity.GetAttributeValue<string>("rlx_archive1text"),
            Archive2Text = entity.GetAttributeValue<string>("rlx_archive2text"),
            Archive3Text = entity.GetAttributeValue<string>("rlx_archive3text"),
        };
    }
}

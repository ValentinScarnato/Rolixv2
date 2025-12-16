using Microsoft.Xrm.Sdk;
using Rolix.Web.Models;

namespace Rolix.Web.Services;

public class QuoteService
{
    private readonly DataverseService _dataverse;

    public QuoteService(DataverseService dataverse)
    {
        _dataverse = dataverse;
    }

    public Guid CreateQuoteRequest(Guid contactId, Product product, string? contactEmail)
    {
        var client = _dataverse.GetClient();

        var task = new Entity("task")
        {
            ["subject"] = $"Demande de devis - {product.Name}",
            ["description"] = BuildDescription(product, contactEmail),
            ["scheduledend"] = DateTime.UtcNow.AddDays(2)
        };

        task["regardingobjectid"] = new EntityReference("contact", contactId);

        return client.Create(task);
    }

    private static string BuildDescription(Product product, string? contactEmail)
    {
        var details = $"Modèle : {product.Name}\nPrix catalogue : {product.Price:C2}\nIdentifiant : {product.Id}";

        if (!string.IsNullOrWhiteSpace(contactEmail))
        {
            details += $"\nEmail client : {contactEmail}";
        }

        if (!string.IsNullOrWhiteSpace(product.Description))
        {
            details += $"\nDescription Dataverse : {product.Description}";
        }

        return details;
    }
}

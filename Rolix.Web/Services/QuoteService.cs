using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Globalization;
using Rolix.Web.Models;

namespace Rolix.Web.Services;

public class QuoteService
{
    private readonly DataverseService _dataverse;

    public QuoteService(DataverseService dataverse)
    {
        _dataverse = dataverse;
    }

    public Guid CreateQuoteRequest(Guid contactId, Product product, string? contactEmail, string? userComment = null)
    {
        var client = _dataverse.GetClient();

        var task = new Entity("task")
        {
            ["subject"] = $"Demande de devis - {product.Name}",
            ["description"] = BuildDescription(product, contactEmail, userComment),
            ["scheduledend"] = DateTime.UtcNow.AddDays(2)
        };

        task["regardingobjectid"] = new EntityReference("contact", contactId);

        return client.Create(task);
    }

    public QuoteCreationResult CreateQuote(Guid contactId, Product product, string? userComment = null, string? contactEmail = null)
    {
        var client = _dataverse.GetClient();

        if (HasPendingQuoteForProduct(contactId, product.Id))
        {
            return new QuoteCreationResult(Guid.Empty, null, "Vous avez déjà un devis en attente pour ce produit.");
        }

        var quote = new Entity("quote")
        {
            ["name"] = $"Devis - {product.Name} - {DateTime.UtcNow.AddHours(1):yyyy-MM-dd HH:mm}",
            ["customerid"] = new EntityReference("contact", contactId),
            ["description"] = BuildDescription(product, contactEmail, userComment),
        };

        var quoteId = client.Create(quote);

        Guid? quoteDetailId = null;
        string? warning = null;

        try
        {
            var productEntity = client.Retrieve(
                "product",
                product.Id,
                new ColumnSet("defaultuomid")
            );

            var uomRef = productEntity?.GetAttributeValue<EntityReference>("defaultuomid");
            if (uomRef == null)
            {
                warning = "Le produit n'a pas de defaultuomid (UoM). Ligne de devis non créée.";
                return new QuoteCreationResult(quoteId, quoteDetailId, warning);
            }

            var quoteDetail = new Entity("quotedetail")
            {
                ["quoteid"] = new EntityReference("quote", quoteId),
                ["productid"] = new EntityReference("product", product.Id),
                ["uomid"] = new EntityReference("uom", uomRef.Id),
                ["quantity"] = 1m,
                ["priceperunit"] = new Money(product.Price),
                ["ispriceoverridden"] = true,
            };

            quoteDetailId = client.Create(quoteDetail);
        }
        catch (Exception ex)
        {
            warning = $"Devis créé mais ligne de devis non créée: {ex.Message}";
        }

        return new QuoteCreationResult(quoteId, quoteDetailId, warning);
    }

    public bool HasPendingQuoteForProduct(Guid contactId, Guid productId)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("quote")
        {
            ColumnSet = new ColumnSet("quoteid"),
            TopCount = 1,
        };

        query.Criteria.AddCondition("customerid", ConditionOperator.Equal, contactId);
        query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

        var link = query.AddLink("quotedetail", "quoteid", "quoteid", JoinOperator.Inner);
        link.LinkCriteria.AddCondition("productid", ConditionOperator.Equal, productId);

        var result = client.RetrieveMultiple(query);
        return result.Entities.Count > 0;
    }

    public List<PendingQuoteSummary> GetPendingQuotesForContact(Guid contactId)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("quote")
        {
            ColumnSet = new ColumnSet("quoteid", "name", "createdon", "statecode", "statuscode"),
            Orders = { new OrderExpression("createdon", OrderType.Descending) },
        };

        query.Criteria.AddCondition("customerid", ConditionOperator.Equal, contactId);
        query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

        var link = query.AddLink("quotedetail", "quoteid", "quoteid", JoinOperator.LeftOuter);
        link.EntityAlias = "qd";
        link.Columns = new ColumnSet("productid", "priceperunit", "quantity");

        var result = client.RetrieveMultiple(query);

        return result.Entities.Select(e =>
        {
            var price = e.GetAttributeValue<AliasedValue>("qd.priceperunit")?.Value as Money;
            var qty = (decimal?)e.GetAttributeValue<AliasedValue>("qd.quantity")?.Value;
            var productRef = e.GetAttributeValue<AliasedValue>("qd.productid")?.Value as EntityReference;

            var amount = (price?.Value ?? 0m) * (qty ?? 0m);

            return new PendingQuoteSummary
            {
                Id = e.Id,
                Name = e.GetAttributeValue<string>("name") ?? string.Empty,
                CreatedOn = e.GetAttributeValue<DateTime?>("createdon"),
                StateCode = e.GetAttributeValue<OptionSetValue>("statecode")?.Value,
                StatusCode = e.GetAttributeValue<OptionSetValue>("statuscode")?.Value,
                ProductId = productRef?.Id,
                ProductName = productRef?.Name,
                Amount = amount,
            };
        }).ToList();
    }

    public List<QuoteRequestSummary> GetQuoteRequestsForContact(Guid contactId)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("task")
        {
            ColumnSet = new ColumnSet(
                "activityid",
                "subject",
                "description",
                "createdon",
                "scheduledend"
            ),
            Orders = { new OrderExpression("createdon", OrderType.Descending) }
        };

        query.Criteria.AddCondition("regardingobjectid", ConditionOperator.Equal, new EntityReference("contact", contactId));
        query.Criteria.AddCondition("subject", ConditionOperator.Like, "Demande de devis%");

        var result = client.RetrieveMultiple(query);

        return result.Entities.Select(e => new QuoteRequestSummary
        {
            Id = e.Id,
            Subject = e.GetAttributeValue<string>("subject") ?? string.Empty,
            CreatedOn = e.GetAttributeValue<DateTime?>("createdon"),
            DueOn = e.GetAttributeValue<DateTime?>("scheduledend"),
            Description = e.GetAttributeValue<string>("description")
        }).ToList();
    }

    private static string BuildDescription(Product product, string? contactEmail, string? userComment)
    {
        var details = $"Modèle : {product.Name}\nPrix catalogue : {product.Price.ToString("C2", CultureInfo.CreateSpecificCulture("fr-CH"))}\nIdentifiant : {product.Id}";

        if (!string.IsNullOrWhiteSpace(contactEmail))
        {
            details += $"\nEmail client : {contactEmail}";
        }

        if (!string.IsNullOrWhiteSpace(product.Description))
        {
            details += $"\nDescription Dataverse : {product.Description}";
        }

        if (!string.IsNullOrWhiteSpace(userComment))
        {
            details += $"\nCommentaire client : {userComment}";
        }

        return details;
    }
}

public class QuoteRequestSummary
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? CreatedOn { get; set; }
    public DateTime? DueOn { get; set; }
    public string? Description { get; set; }
}

public record QuoteCreationResult(Guid QuoteId, Guid? QuoteDetailId, string? Warning);

public class PendingQuoteSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? CreatedOn { get; set; }
    public int? StateCode { get; set; }
    public int? StatusCode { get; set; }

    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal Amount { get; set; }
}

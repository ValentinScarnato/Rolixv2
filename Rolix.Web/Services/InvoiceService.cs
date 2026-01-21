using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rolix.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;

namespace Rolix.Web.Services
{
    public class InvoiceService
    {
        private readonly DataverseService _dataverse;
        public InvoiceService(DataverseService dataverse)
        {
            _dataverse = dataverse;
        }

        public List<Invoice> GetInvoicesForContact(Guid contactId)
        {
            var client = _dataverse.GetClient();
            var query = new QueryExpression("invoice")
            {
                ColumnSet = new ColumnSet("invoiceid", "invoicenumber", "totalamount", "statuscode", "createdon"),
            };

            query.Criteria.AddCondition("customerid", ConditionOperator.Equal, contactId);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));

            var result = client.RetrieveMultiple(query);
            return result.Entities.Select(MapInvoice).ToList();
        }

        private static Invoice MapInvoice(Entity e)
        {
            return new Invoice
            {
                Id = e.Id,
                Number = e.GetAttributeValue<string>("invoicenumber") ?? string.Empty,
                Date = e.GetAttributeValue<DateTime?>("createdon"),
                Amount = e.GetAttributeValue<Money>("totalamount")?.Value ?? 0,
                Status = e.FormattedValues.Contains("statuscode") ? e.FormattedValues["statuscode"] : string.Empty,
                PdfUrl = null // À implémenter si un champ ou une méthode fournit l’URL du PDF
            };
        }
        public byte[]? GetInvoicePdf(Guid invoiceId)
        {
            // Génère un PDF de test pour vérifier la chaîne complète
            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Content().Text($"Test PDF pour facture {invoiceId}");
                });
            }).GeneratePdf();
            return pdf;
        }
    }
}

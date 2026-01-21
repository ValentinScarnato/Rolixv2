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
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public InvoiceService(DataverseService dataverse, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _dataverse = dataverse;
            _env = env;
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
            var client = _dataverse.GetClient();
            
            // Récupérer la facture avec tous les champs nécessaires
            Entity invoice;
            try
            {
                invoice = client.Retrieve("invoice", invoiceId, 
                    new ColumnSet("invoicenumber", "totalamount", "createdon", 
                                  "statuscode", "customerid", "name"));
            }
            catch
            {
                return null;
            }
            
            // Récupérer le client
            var customerId = invoice.GetAttributeValue<EntityReference>("customerid");
            Entity? contact = null;
            if (customerId != null)
            {
                try
                {
                    contact = client.Retrieve("contact", customerId.Id, 
                        new ColumnSet("fullname", "emailaddress2", "address1_line1", 
                                      "address1_city", "address1_postalcode", "address1_country"));
                }
                catch
                {
                    // Continue sans les infos du contact
                }
            }
            
            // Extraire les données
            var invoiceNumber = invoice.GetAttributeValue<string>("invoicenumber") ?? "N/A";
            var invoiceName = invoice.GetAttributeValue<string>("name") ?? "";
            var totalAmount = invoice.GetAttributeValue<Money>("totalamount")?.Value ?? 0;
            var createdOn = invoice.GetAttributeValue<DateTime?>("createdon") ?? DateTime.Now;
            var status = invoice.FormattedValues.Contains("statuscode") ? invoice.FormattedValues["statuscode"] : "N/A";
            
            var customerName = contact?.GetAttributeValue<string>("fullname") ?? "Client";
            var customerEmail = contact?.GetAttributeValue<string>("emailaddress2") ?? "";
            var customerAddress = contact?.GetAttributeValue<string>("address1_line1") ?? "";
            var customerCity = contact?.GetAttributeValue<string>("address1_city") ?? "";
            var customerPostalCode = contact?.GetAttributeValue<string>("address1_postalcode") ?? "";
            var customerCountry = contact?.GetAttributeValue<string>("address1_country") ?? "";
            
            // Générer le PDF avec QuestPDF
            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(210, 297, QuestPDF.Infrastructure.Unit.Millimetre); // A4
                    page.Margin(40);
                    
                    page.Header().Column(column =>
                    {
                        column.Item().Text("ROLIX")
                            .FontSize(24)
                            .Bold()
                            .FontColor("#D4AF37");
                        
                        column.Item().Text("Facture")
                            .FontSize(12)
                            .FontColor("#666666");
                        
                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor("#CCCCCC");
                    });
                    
                    page.Content().PaddingTop(20).Column(column =>
                    {
                        // Informations de la facture
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Facturé à :").Bold().FontSize(10);
                                col.Item().PaddingTop(3).Text(customerName).FontSize(9);
                                if (!string.IsNullOrEmpty(customerEmail))
                                    col.Item().Text(customerEmail).FontSize(8).FontColor("#666666");
                                if (!string.IsNullOrEmpty(customerAddress))
                                    col.Item().Text(customerAddress).FontSize(8);
                                if (!string.IsNullOrEmpty(customerCity) || !string.IsNullOrEmpty(customerPostalCode))
                                    col.Item().Text($"{customerPostalCode} {customerCity}".Trim()).FontSize(8);
                                if (!string.IsNullOrEmpty(customerCountry))
                                    col.Item().Text(customerCountry).FontSize(8);
                            });
                            
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignRight().Text($"Facture N° {invoiceNumber}").Bold().FontSize(10);
                                col.Item().AlignRight().PaddingTop(3).Text($"Date : {createdOn:dd/MM/yyyy}").FontSize(9);
                                col.Item().AlignRight().Text($"Statut : {status}").FontSize(9).FontColor("#666666");
                            });
                        });
                        
                        // Ligne de séparation
                        column.Item().PaddingVertical(15).LineHorizontal(1).LineColor("#CCCCCC");
                        
                        // Description si disponible
                        if (!string.IsNullOrEmpty(invoiceName))
                        {
                            column.Item().PaddingBottom(10).Column(col =>
                            {
                                col.Item().Text("Description").Bold().FontSize(10);
                                col.Item().PaddingTop(3).Text(invoiceName).FontSize(9);
                            });
                        }
                        
                        // Montant
                        column.Item().PaddingTop(10).Column(col =>
                        {
                            col.Item().Text("Montant total").Bold().FontSize(10);
                            col.Item().PaddingTop(3).Text(totalAmount.ToString("C2", System.Globalization.CultureInfo.CreateSpecificCulture("fr-CH")))
                                .FontSize(9);
                        });
                        
                        // Total final
                        column.Item().PaddingTop(20).AlignRight().Row(row =>
                        {
                            row.AutoItem().PaddingRight(10).Text("TOTAL :").Bold().FontSize(16);
                            row.AutoItem().Text(totalAmount.ToString("C2", System.Globalization.CultureInfo.CreateSpecificCulture("fr-CH")))
                                .Bold().FontSize(16).FontColor("#D4AF37");
                        });
                    });
                    
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Merci pour votre confiance - ").FontSize(8).FontColor("#999999");
                        text.Span("ROLIX").Bold().FontSize(8).FontColor("#D4AF37");
                    });
                });
            }).GeneratePdf();
            
            return pdf;
        }
    }
}

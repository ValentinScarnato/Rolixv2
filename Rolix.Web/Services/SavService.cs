using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rolix.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rolix.Web.Services
{
    public class SavService
    {
        private readonly DataverseService _dataverse;

        public SavService(DataverseService dataverse)
        {
            _dataverse = dataverse;
        }

        public List<SavRequest> GetSavRequestsForContact(Guid contactId)
        {
            var client = _dataverse.GetClient();
            var query = new QueryExpression("rlx_retoursav")
            {
                ColumnSet = new ColumnSet("rlx_retoursavid", "rlx_name", "rlx_montreconcernee", 
                                          "rlx_datedereception", "rlx_diagnostique"),
            };

            query.Criteria.AddCondition("rlx_client", ConditionOperator.Equal, contactId);
            query.Orders.Add(new OrderExpression("rlx_datedereception", OrderType.Descending));

            var result = client.RetrieveMultiple(query);
            return result.Entities.Select(MapSavRequest).ToList();
        }

        public Guid CreateSavRequest(Guid contactId, Guid productId, string watchModel, string diagnostic)
        {
            var client = _dataverse.GetClient();

            // Générer le numéro de ticket
            var ticketNumber = GenerateTicketNumber();

            var savRequest = new Entity("rlx_retoursav");
            savRequest["rlx_name"] = ticketNumber;
            savRequest["rlx_montreconcernee"] = new EntityReference("product", productId);
            savRequest["rlx_client"] = new EntityReference("contact", contactId);
            savRequest["rlx_diagnostique"] = diagnostic;
            savRequest["rlx_datedereception"] = DateTime.Now;

            return client.Create(savRequest);
        }

        private string GenerateTicketNumber()
        {
            // Générer un numéro aléatoire à 3 chiffres
            var random = new Random();
            var number = random.Next(100, 1000);
            return $"SAV-{number}";
        }

        private static SavRequest MapSavRequest(Entity e)
        {
            // Récupérer le nom du produit depuis l'EntityReference
            var watchModelRef = e.GetAttributeValue<EntityReference>("rlx_montreconcernee");
            var watchModel = watchModelRef?.Name ?? string.Empty;

            return new SavRequest
            {
                Id = e.Id,
                TicketNumber = e.GetAttributeValue<string>("rlx_name") ?? string.Empty,
                WatchModel = watchModel,
                ReceptionDate = e.GetAttributeValue<DateTime?>("rlx_datedereception") ?? DateTime.Now,
                Diagnostic = e.GetAttributeValue<string>("rlx_diagnostique"),
                ProductId = watchModelRef?.Id
            };
        }
    }
}

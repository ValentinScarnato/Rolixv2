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
                                          "rlx_datedereception", "rlx_diagnostique", "rlx_statut", "rlx_client", "rlx_noteclient"),
            };

            // Filtre Dataverse (Should work)
            query.Criteria.AddCondition("rlx_client", ConditionOperator.Equal, contactId);
            query.Orders.Add(new OrderExpression("rlx_datedereception", OrderType.Descending));

            // Force no-cache / latest data
            query.PageInfo = new PagingInfo { Count = 50, PageNumber = 1 };

            var result = client.RetrieveMultiple(query);
            
            // Filtre C# de sécurité absolue
            var filteredRequests = new List<SavRequest>();
            
            foreach (var entity in result.Entities)
            {
                var clientRef = entity.GetAttributeValue<EntityReference>("rlx_client");
                
                // Si pas de client défini ou ID différent, on saute (Sécurité critique)
                if (clientRef == null || clientRef.Id != contactId)
                    continue;

                filteredRequests.Add(MapSavRequest(entity));
            }

            return filteredRequests;
        }


        public bool HasSavRequestForProduct(Guid contactId, Guid productId)
        {
            var client = _dataverse.GetClient();
            var query = new QueryExpression("rlx_retoursav")
            {
                ColumnSet = new ColumnSet("rlx_retoursavid", "rlx_client")
            };

            query.Criteria.AddCondition("rlx_client", ConditionOperator.Equal, contactId);
            query.Criteria.AddCondition("rlx_montreconcernee", ConditionOperator.Equal, productId);
            
            // On ne peut avoir qu'une seule demande en cours ou terminée
            // Si on voulait autoriser une nouvelle demande si l'ancienne est terminée, on filtrerait sur le statut
            // Ici l'exigence est "une seule demande de sav par produit"

            var result = client.RetrieveMultiple(query);
            
            // Vérification stricte en mémoire
            return result.Entities.Any(e => 
            {
                var clientRef = e.GetAttributeValue<EntityReference>("rlx_client");
                return clientRef != null && clientRef.Id == contactId;
            });
        }

        public Guid CreateSavRequest(Guid contactId, Guid productId, string watchModel, string diagnostic)
        {
            // Validation: vérifier si une demande existe déjà
            if (HasSavRequestForProduct(contactId, productId))
            {
                throw new InvalidOperationException("Une demande SAV existe déjà pour ce produit.");
            }

            var client = _dataverse.GetClient();

            // Générer le numéro de ticket
            var ticketNumber = GenerateTicketNumber();

            var savRequest = new Entity("rlx_retoursav");
            savRequest["rlx_name"] = ticketNumber;
            savRequest["rlx_montreconcernee"] = new EntityReference("product", productId);
            savRequest["rlx_client"] = new EntityReference("contact", contactId);
            savRequest["rlx_diagnostique"] = diagnostic;
            savRequest["rlx_datedereception"] = DateTime.Now;
            savRequest["rlx_statut"] = new OptionSetValue(912460002); // 912460002 = En Attente

            return client.Create(savRequest);
        }

        public void UpdateSavRequestRating(Guid requestId, int rating)
        {
            if (rating < 1 || rating > 5) return;

            var client = _dataverse.GetClient();
            var savRequest = new Entity("rlx_retoursav", requestId);
            savRequest["rlx_noteclient"] = rating;
            client.Update(savRequest);
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
                ProductId = watchModelRef?.Id,
                Status = e.FormattedValues.Contains("rlx_statut") ? e.FormattedValues["rlx_statut"] : "En cours",
                CustomerRating = e.GetAttributeValue<int?>("rlx_noteclient")
            };
        }
    }
}

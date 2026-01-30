using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rolix.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rolix.Web.Services
{
    /// <summary>
    /// Service de gestion des demandes de retour SAV (Service Après-Vente).
    /// Permet de créer, récupérer et évaluer les demandes SAV pour les produits achetés.
    /// Inclut une sécurité stricte pour garantir que chaque client ne voit que ses propres demandes.
    /// </summary>
    public class SavService
    {
        private readonly DataverseService _dataverse;

        /// <summary>
        /// Initialise une nouvelle instance du service SAV.
        /// </summary>
        /// <param name="dataverse">Service de connexion à Dataverse</param>
        public SavService(DataverseService dataverse)
        {
            _dataverse = dataverse;
        }

        /// <summary>
        /// Récupère toutes les demandes SAV d'un client spécifique.
        /// Applique un double filtrage (Dataverse + C#) pour garantir la sécurité des données.
        /// </summary>
        /// <param name="contactId">ID du contact client dans Dataverse</param>
        /// <returns>Liste des demandes SAV filtrées et triées par date de réception décroissante</returns>
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


        /// <summary>
        /// Vérifie si un client a déjà une demande SAV pour un produit spécifique.
        /// Utilisé pour empêcher les demandes SAV en double pour le même produit.
        /// </summary>
        /// <param name="contactId">ID du contact client</param>
        /// <param name="productId">ID du produit concerné</param>
        /// <returns>True si une demande SAV existe déjà, False sinon</returns>
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

        /// <summary>
        /// Crée une nouvelle demande SAV pour un produit acheté.
        /// Génère automatiquement un numéro de ticket et définit le statut initial à "En Attente".
        /// </summary>
        /// <param name="contactId">ID du contact client</param>
        /// <param name="productId">ID du produit concerné</param>
        /// <param name="watchModel">Nom/modèle de la montre</param>
        /// <param name="diagnostic">Description du problème fournie par le client</param>
        /// <returns>ID de la demande SAV créée</returns>
        /// <exception cref="InvalidOperationException">Lancée si une demande SAV existe déjà pour ce produit</exception>
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

        /// <summary>
        /// Met à jour la note client d'une demande SAV.
        /// </summary>
        /// <param name="requestId">ID de la demande SAV à évaluer</param>
        /// <param name="rating">Note de 1 à 5 étoiles (valeurs hors plage sont ignorées)</param>
        public void UpdateSavRequestRating(Guid requestId, int rating)
        {
            if (rating < 1 || rating > 5) return;

            var client = _dataverse.GetClient();
            var savRequest = new Entity("rlx_retoursav", requestId);
            savRequest["rlx_noteclient"] = rating;
            client.Update(savRequest);
        }

        /// <summary>
        /// Génère un numéro de ticket unique pour une demande SAV.
        /// Format: SAV-XXX où XXX est un nombre aléatoire entre 100 et 999.
        /// </summary>
        /// <returns>Numéro de ticket au format SAV-XXX</returns>
        private string GenerateTicketNumber()
        {
            // Générer un numéro aléatoire à 3 chiffres
            var random = new Random();
            var number = random.Next(100, 1000);
            return $"SAV-{number}";
        }

        /// <summary>
        /// Convertit une entité Dataverse "rlx_retoursav" en objet SavRequest.
        /// </summary>
        /// <param name="e">Entité rlx_retoursav de Dataverse</param>
        /// <returns>Objet SavRequest avec les informations de la demande SAV</returns>
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

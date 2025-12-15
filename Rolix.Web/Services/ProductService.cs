using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rolix.Web.Models;

namespace Rolix.Web.Services;

public class ProductService
{
    private readonly DataverseService _dataverse;

    public ProductService(DataverseService dataverse)
    {
        _dataverse = dataverse;
    }

    // 🔹 LISTE DES PRODUITS
    public List<Product> GetAll()
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("product")
        {
            ColumnSet = new ColumnSet(
                "productid",
                "name",
                "price",
                "entityimage"
            )
        };

        var result = client.RetrieveMultiple(query);

        return result.Entities.Select(MapProduct).ToList();
    }
    // ---------------------------------------------------------
    // 2. NOUVELLE MÉTHODE (Uniquement pour l'Accueil)
    // ---------------------------------------------------------
    public List<Product> GetTopExpensiveProducts(int count)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("product")
        {
            // On récupère aussi la DESCRIPTION ici
            ColumnSet = new ColumnSet("productid", "name", "price", "description", "entityimage"),

            // On limite le nombre de résultats (ex: 2)
            TopCount = count,

            // On trie par prix décroissant (du plus cher au moins cher)
            Orders = { new OrderExpression("price", OrderType.Descending) }
        };

        var result = client.RetrieveMultiple(query);
        return result.Entities.Select(MapProduct).ToList();
    }

    // 🔹 DÉTAIL D’UN PRODUIT
    public Product? GetById(Guid id)
    {
        var client = _dataverse.GetClient();

        var entity = client.Retrieve(
            "product",
            id,
            new ColumnSet(
                "productid",
                "name",
                "price",
                "description",
                "entityimage"
            )
        );

        return entity != null ? MapProduct(entity) : null;
    }

    // 🔹 Mapping centralisé
    private static Product MapProduct(Entity e)
    {
        var imageBytes = e.GetAttributeValue<byte[]>("entityimage");

        return new Product
        {
            Id = e.Id,
            Name = e.GetAttributeValue<string>("name") ?? "",
            Description = e.GetAttributeValue<string>("description"),
            Price = e.GetAttributeValue<Money>("price")?.Value ?? 0,
            ImageBase64 = imageBytes != null
                ? $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}"
                : null
        };
    }

    public List<Product> GetFiltered(string search, string sortOrder, int? category)
    {
        var client = _dataverse.GetClient();
        var query = new QueryExpression("product");

        query.ColumnSet = new ColumnSet("productid", "name", "price", "entityimage");

        // 1. FILTRE OBLIGATOIRE : PRIX STRICTEMENT POSITIF (> 0)
        query.Criteria.AddCondition("price", ConditionOperator.GreaterThan, 0);

        // 2. FILTRE RECHERCHE (Par nom)
        if (!string.IsNullOrEmpty(search))
        {
            query.Criteria.AddCondition("name", ConditionOperator.Like, $"%{search}%");
        }

        // 3. FILTRE CATÉGORIE (Structure Produit)
        // On vérifie si une catégorie a été sélectionnée (donc non null)
        if (category.HasValue)
        {
            query.Criteria.AddCondition("productstructure", ConditionOperator.Equal, category.Value);
        }

        // 4. TRI
        switch (sortOrder)
        {
            case "price_desc":
                query.Orders.Add(new OrderExpression("price", OrderType.Descending));
                break;
            case "price_asc":
                query.Orders.Add(new OrderExpression("price", OrderType.Ascending));
                break;
            case "name_asc":
            default:
                query.Orders.Add(new OrderExpression("name", OrderType.Ascending));
                break;
        }

        var result = client.RetrieveMultiple(query);
        return result.Entities.Select(MapProduct).ToList();
    }

}

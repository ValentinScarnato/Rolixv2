using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rolix.Web.Models;

namespace Rolix.Web.Services;

public class ProductService
{
    private readonly DataverseService _dataverse;

    private const int ProductStructureFamily = 2;

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
                "entityimage"
            )
        };

        query.Criteria.AddCondition("productstructure", ConditionOperator.NotEqual, ProductStructureFamily);

        var result = client.RetrieveMultiple(query);

        var products = result.Entities.Select(MapProduct).ToList();
        ApplyPricesFromPriceListItems(products);
        return products.Where(p => p.Price > 0).ToList();
    }

    public List<Product> GetFamilies()
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("product")
        {
            ColumnSet = new ColumnSet(
                "productid",
                "name"
            )
        };

        query.Criteria.AddCondition("productstructure", ConditionOperator.Equal, ProductStructureFamily);
        query.Orders.Add(new OrderExpression("name", OrderType.Ascending));

        var result = client.RetrieveMultiple(query);

        return result.Entities
            .Select(e => new Product
            {
                Id = e.Id,
                Name = e.GetAttributeValue<string>("name") ?? string.Empty,
                Price = 0,
            })
            .ToList();
    }
    // ---------------------------------------------------------
    // 2. NOUVELLE MÉTHODE (Uniquement pour l'Accueil)
    // ---------------------------------------------------------
    public List<Product> GetTopExpensiveProducts(int count)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("product")
        {
            // On récupère nom, description et image
            ColumnSet = new ColumnSet("productid", "name", "description", "entityimage"),

            // IMPORTANT : On retire TopCount ici pour scanner tout le catalogue
            // sinon on rate les montres chères si elles ne sont pas dans les premières lignes
        };

        query.Criteria.AddCondition("productstructure", ConditionOperator.NotEqual, ProductStructureFamily);

        var result = client.RetrieveMultiple(query);

        var products = result.Entities.Select(MapProduct).ToList();

        // 1. On récupère les prix
        ApplyPricesFromPriceListItems(products);

        // 2. On trie par prix décroissant ET on prend les 'count' premiers
        return products
            .Where(p => p.Price > 0)
            .OrderByDescending(p => p.Price)
            .Take(count)
            .ToList();
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
                "description",
                "entityimage"
            )
        );

        if (entity == null)
        {
            return null;
        }

        var product = MapProduct(entity);
        product.Price = GetPriceForProduct(product.Id);
        return product;
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
            Price = 0,
            ImageBase64 = imageBytes != null
                ? $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}"
                : null
        };
    }

    public List<Product> GetFiltered(string search, string sortOrder, Guid? familyId)
    {
        var client = _dataverse.GetClient();
        var query = new QueryExpression("product");

        query.ColumnSet = new ColumnSet("productid", "name", "entityimage");

        query.Criteria.AddCondition("productstructure", ConditionOperator.NotEqual, ProductStructureFamily);

        // 1. FILTRE RECHERCHE (Par nom)
        if (!string.IsNullOrEmpty(search))
        {
            query.Criteria.AddCondition("name", ConditionOperator.Like, $"%{search}%");
        }

        // 2. FILTRE FAMILLE (produit parent)
        if (familyId.HasValue)
        {
            query.Criteria.AddCondition("parentproductid", ConditionOperator.Equal, familyId.Value);
        }

        // 3. TRI
        var result = client.RetrieveMultiple(query);

        var products = result.Entities.Select(MapProduct).ToList();
        ApplyPricesFromPriceListItems(products);
        products = products.Where(p => p.Price > 0).ToList();

        return sortOrder switch
        {
            "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
            "price_asc" => products.OrderBy(p => p.Price).ToList(),
            _ => products.OrderBy(p => p.Name).ToList(),
        };
    }

    private decimal GetPriceForProduct(Guid productId)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("productpricelevel")
        {
            ColumnSet = new ColumnSet("amount", "productid", "createdon"),
            TopCount = 1,
            Orders = { new OrderExpression("createdon", OrderType.Descending) },
        };

        query.Criteria.AddCondition("productid", ConditionOperator.Equal, productId);

        var result = client.RetrieveMultiple(query);
        var entity = result.Entities.FirstOrDefault();

        return entity?.GetAttributeValue<Money>("amount")?.Value ?? 0m;
    }

    private void ApplyPricesFromPriceListItems(List<Product> products)
    {
        if (products.Count == 0)
        {
            return;
        }

        var client = _dataverse.GetClient();
        var productIds = products.Select(p => p.Id).Distinct().ToArray();
        var productIdValues = productIds.Cast<object>().ToArray();

        var query = new QueryExpression("productpricelevel")
        {
            ColumnSet = new ColumnSet("amount", "productid", "createdon"),
            Orders = { new OrderExpression("createdon", OrderType.Descending) },
        };

        query.Criteria.AddCondition("productid", ConditionOperator.In, productIdValues);

        var result = client.RetrieveMultiple(query);
        var pricesByProductId = new Dictionary<Guid, decimal>();

        foreach (var entity in result.Entities)
        {
            var productRef = entity.GetAttributeValue<EntityReference>("productid");
            if (productRef == null)
            {
                continue;
            }

            if (pricesByProductId.ContainsKey(productRef.Id))
            {
                continue;
            }

            var amount = entity.GetAttributeValue<Money>("amount")?.Value ?? 0m;
            pricesByProductId[productRef.Id] = amount;
        }

        foreach (var product in products)
        {
            if (pricesByProductId.TryGetValue(product.Id, out var amount))
            {
                product.Price = amount;
            }
        }
    }

}

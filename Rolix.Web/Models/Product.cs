namespace Rolix.Web.Models;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? ImageBase64 { get; set; }

    public string? Mouvement { get; set; } // rlx_mouvement
    public string? Materiaux { get; set; } // rlx_materiaux
    public int? Etancheite { get; set; } // rlx_etancheitem
    public string? Dimensions { get; set; } // rlx_dimensions
}

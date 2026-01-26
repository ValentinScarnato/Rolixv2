using System;

namespace Rolix.Web.Models
{
    public class PurchasedProduct
    {
        public Guid InvoiceDetailId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal Amount { get; set; }
        public bool HasSavRequest { get; set; }
    }
}

using System;

namespace Rolix.Web.Models
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PdfUrl { get; set; }
    }
}

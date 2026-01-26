using System;

namespace Rolix.Web.Models
{
    public class SavRequest
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string WatchModel { get; set; } = string.Empty;
        public DateTime ReceptionDate { get; set; }
        public string? Diagnostic { get; set; }
        public Guid? ProductId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? CustomerRating { get; set; }
    }
}

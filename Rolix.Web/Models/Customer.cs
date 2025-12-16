namespace Rolix.Web.Models
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        // Helper pour afficher le nom complet
        public string DisplayName => $"{FirstName} {LastName}".Trim();
    }
}

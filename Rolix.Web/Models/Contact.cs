namespace Rolix.Web.Models;

public class Contact
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }

    public string DisplayName => $"{FirstName} {LastName}".Trim();
}

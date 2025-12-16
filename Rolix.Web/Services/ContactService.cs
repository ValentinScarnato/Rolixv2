using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rolix.Web.Models;

namespace Rolix.Web.Services;

public class ContactService
{
    private readonly DataverseService _dataverse;

    public ContactService(DataverseService dataverse)
    {
        _dataverse = dataverse;
    }

    public Contact? Authenticate(string email, string? lastName)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet("contactid", "firstname", "lastname", "emailaddress1"),
        };

        query.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, email);

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            query.Criteria.AddCondition("lastname", ConditionOperator.Equal, lastName);
        }

        var result = client.RetrieveMultiple(query);
        var entity = result.Entities.FirstOrDefault();

        return entity != null ? MapContact(entity) : null;
    }

    public Contact? GetByEmail(string email)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet("contactid", "firstname", "lastname", "emailaddress1"),
        };

        query.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, email);

        var result = client.RetrieveMultiple(query);
        var entity = result.Entities.FirstOrDefault();

        return entity != null ? MapContact(entity) : null;
    }

    public Contact Create(Contact contact)
    {
        var client = _dataverse.GetClient();

        var entity = new Entity("contact");

        if (!string.IsNullOrWhiteSpace(contact.FirstName))
        {
            entity["firstname"] = contact.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(contact.LastName))
        {
            entity["lastname"] = contact.LastName;
        }

        if (!string.IsNullOrWhiteSpace(contact.Email))
        {
            entity["emailaddress1"] = contact.Email;
        }

        var id = client.Create(entity);

        return new Contact
        {
            Id = id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
        };
    }

    private static Contact MapContact(Entity entity)
    {
        return new Contact
        {
            Id = entity.Id,
            FirstName = entity.GetAttributeValue<string>("firstname"),
            LastName = entity.GetAttributeValue<string>("lastname"),
            Email = entity.GetAttributeValue<string>("emailaddress1"),
        };
    }
}

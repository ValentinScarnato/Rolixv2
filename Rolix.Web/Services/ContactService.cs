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

        if (entity == null)
        {
            return null;
        }

        return new Contact
        {
            Id = entity.Id,
            FirstName = entity.GetAttributeValue<string>("firstname"),
            LastName = entity.GetAttributeValue<string>("lastname"),
            Email = entity.GetAttributeValue<string>("emailaddress1"),
        };
    }
}

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

    public Contact? Authenticate(string username, string password)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet(
                "contactid",
                "firstname",
                "lastname",
                "fullname",
                "emailaddress2",
                "adx_identity_username",
                "rlx_password"
            ),
        };

        query.Criteria.AddCondition("adx_identity_username", ConditionOperator.Equal, username);
        query.Criteria.AddCondition("rlx_password", ConditionOperator.Equal, password);

        var result = client.RetrieveMultiple(query);
        var entity = result.Entities.FirstOrDefault();

        if (entity == null)
        {
            return null;
        }

        return MapContact(entity);
    }

    public Contact? GetById(Guid contactId)
    {
        var client = _dataverse.GetClient();

        var entity = client.Retrieve(
            "contact",
            contactId,
            new ColumnSet(
                "contactid",
                "firstname",
                "lastname",
                "fullname",
                "emailaddress2",
                "adx_identity_username"
            )
        );

        return MapContact(entity);
    }

    public Contact? GetByEmail(string email)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet(
                "contactid",
                "firstname",
                "lastname",
                "fullname",
                "emailaddress2",
                "adx_identity_username"
            ),
        };

        query.Criteria.AddCondition("emailaddress2", ConditionOperator.Equal, email);

        var result = client.RetrieveMultiple(query);
        var entity = result.Entities.FirstOrDefault();

        if (entity == null)
        {
            return null;
        }

        return MapContact(entity);
    }

    public Contact? GetByUsername(string username)
    {
        var client = _dataverse.GetClient();

        var query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet(
                "contactid",
                "firstname",
                "lastname",
                "fullname",
                "emailaddress2",
                "adx_identity_username"
            ),
        };

        query.Criteria.AddCondition("adx_identity_username", ConditionOperator.Equal, username);

        var result = client.RetrieveMultiple(query);
        var entity = result.Entities.FirstOrDefault();

        return entity == null ? null : MapContact(entity);
    }

    public Guid Create(string fullname, string email, string username, string password)
    {
        var client = _dataverse.GetClient();

        var entity = new Entity("contact");
        entity["fullname"] = fullname;
        entity["emailaddress2"] = email;
        entity["adx_identity_username"] = username;
        entity["rlx_password"] = password;

        return client.Create(entity);
    }

    public void Update(Guid contactId, string fullname, string email)
    {
        var client = _dataverse.GetClient();

        var entity = new Entity("contact")
        {
            Id = contactId,
        };

        entity["fullname"] = fullname;
        entity["emailaddress2"] = email;

        client.Update(entity);
    }

    private static Contact MapContact(Entity entity)
    {
        return new Contact
        {
            Id = entity.Id,
            FirstName = entity.GetAttributeValue<string>("firstname"),
            LastName = entity.GetAttributeValue<string>("lastname"),
            FullName = entity.GetAttributeValue<string>("fullname"),
            Email = entity.GetAttributeValue<string>("emailaddress2"),
            Username = entity.GetAttributeValue<string>("adx_identity_username"),
        };
    }
}

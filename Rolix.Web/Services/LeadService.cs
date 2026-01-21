using Microsoft.Xrm.Sdk;
using System;

namespace Rolix.Web.Services
{
    public class LeadService
    {
        private readonly DataverseService _dataverse;

        public LeadService(DataverseService dataverse)
        {
            _dataverse = dataverse;
        }

        public Guid CreateLead(string firstName, string lastName, string email, string subject, string description)
        {
            var client = _dataverse.GetClient();

            var lead = new Entity("lead");
            lead["firstname"] = firstName;
            lead["lastname"] = lastName;
            lead["emailaddress1"] = email;
            lead["subject"] = subject;
            lead["description"] = description;

            return client.Create(lead);
        }
    }
}

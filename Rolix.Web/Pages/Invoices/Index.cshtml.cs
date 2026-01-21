using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Rolix.Web.Models;
using Rolix.Web.Services;
using System;
using System.Collections.Generic;

namespace Rolix.Web.Pages.Invoices
{
    public class IndexModel : PageModel
    {
        private readonly InvoiceService _invoiceService;
        private readonly ContactService _contactService;
        public List<Invoice> Invoices { get; set; } = new();

        public IndexModel(InvoiceService invoiceService, ContactService contactService)
        {
            _invoiceService = invoiceService;
            _contactService = contactService;
        }

        public IActionResult OnGet()
        {
            var contactIdStr = HttpContext.Session.GetString("ContactId");
            if (!Guid.TryParse(contactIdStr, out var contactId))
                return RedirectToPage("/Account/Index");

            Invoices = _invoiceService.GetInvoicesForContact(contactId);
            return Page();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolix.Web.Services;
using System;

namespace Rolix.Web.Pages.Invoices
{
    public class DownloadModel : PageModel
    {
        private readonly InvoiceService _invoiceService;
        public DownloadModel(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public IActionResult OnGet(Guid id)
        {
            // À adapter selon où/comment est stocké le PDF
            var pdfBytes = _invoiceService.GetInvoicePdf(id);
            if (pdfBytes == null)
                return NotFound();
            return File(pdfBytes, "application/pdf", $"facture_{id}.pdf");
        }
    }
}

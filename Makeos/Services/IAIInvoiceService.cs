using Makeos.Models;

namespace Makeos.Services
{
    public interface IAIInvoiceService
    {
        Task<InvoiceInfo> ExtractInvoiceInfoAsync(PDFInfo pdfInfo);
    }
}

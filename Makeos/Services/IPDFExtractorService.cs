using Makeos.Models;

namespace Makeos.Services
{
    public interface IPDFExtractorService
    {
        Task<PDFInfo> ExtractTextAsync(IFormFile file);
    }
}
